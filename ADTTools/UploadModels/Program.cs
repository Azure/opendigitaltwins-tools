using ADTToolsLibrary;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using CommandLine;
using Ganss.IO;
using DTDLParser;
using DTDLParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;

namespace UploadModels
{
    class Program
    {
        private readonly Options options;

        static async Task Main(string[] args)
        {
            await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(options => new Program(options).Run());
        }

        private Program(Options options)
        {
            this.options = options;
        }

        private async Task Run()
        {
            // Expand globs and wildcards for all file specs.
            IEnumerable<string> fileNames = GetFileNames(options.FileSpecs);

            // Load all the model text.
            var modelTexts = new Dictionary<string, string>();
            foreach (string fileName in fileNames)
            {
                modelTexts.Add(fileName, File.ReadAllText(fileName));
                Log.Write($"Loaded: {fileName}");
            }

            Log.Write(string.Empty);

            // Check that all model text is valid JSON (for better error reporting).
            if (!ParseModelJson(modelTexts))
            {
                return;
            }

            // Parse models.
            IReadOnlyDictionary<Dtmi, DTEntityInfo> entities = await ParseModelsAsync(modelTexts);
            if (entities == null)
            {
                return;
            }

            // Get interfaces.
            IEnumerable<DTInterfaceInfo> interfaces = from entity in entities.Values
                                                      where entity.EntityKind == DTEntityKind.Interface
                                                      select (DTInterfaceInfo)entity;
            Log.Ok($"Parsed {interfaces.Count()} models successfully.");
            Log.Write(string.Empty);

            // Order interfaces for upload.
            IEnumerable<DTInterfaceInfo> orderedInterfaces = OrderInterfaces(interfaces);

            if (options.WhatIf)
            {
                DisplayOrderedInterfaces(orderedInterfaces);
            }
            else
            {
                await UploadOrderedInterfaces(orderedInterfaces);
            }
        }

        private async Task UploadOrderedInterfaces(IEnumerable<DTInterfaceInfo> orderedInterfaces)
        {
            Log.Write("Uploaded interfaces:");
            try
            {
                TokenCredential credential;
                if (options.UseDefaultAzureCredentials)
                {
                    credential = new DefaultAzureCredential();
                }
                else
                {
                    if (string.IsNullOrEmpty(options.ClientSecret))
                    {
                        credential = new InteractiveBrowserCredential(options.TenantId, options.ClientId);
                    }
                    else
                    {
                        credential = new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
                    }
                }

                var client = new DigitalTwinsClient(new UriBuilder("https", options.HostName).Uri, credential);

                for (int i = 0; i < (orderedInterfaces.Count() / options.BatchSize) + 1; i++)
                {
                    IEnumerable<DTInterfaceInfo> batch = orderedInterfaces.Skip(i * options.BatchSize).Take(options.BatchSize);
                    Response<DigitalTwinsModelData[]> response = await client.CreateModelsAsync(batch.Select(i => i.GetJsonLdText()));
                    foreach (DTInterfaceInfo @interface in batch)
                    {
                        Log.Ok(@interface.Id.AbsoluteUri);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Upload failed.");
                Log.Error(ex.Message);
            }
        }

        private static void DisplayOrderedInterfaces(IEnumerable<DTInterfaceInfo> orderedInterfaces)
        {
            Log.Write("Ordered interfaces:");
            foreach (DTInterfaceInfo orderedInterface in orderedInterfaces)
            {
                Log.Write(orderedInterface.Id.AbsoluteUri);
            }

        }

        private static bool ParseModelJson(Dictionary<string, string> modelTexts)
        {
            var jsonErrors = new Dictionary<string, System.Text.Json.JsonException>();
            foreach (string fileName in modelTexts.Keys)
            {
                try
                {
                    JsonDocument jsonDoc = JsonDocument.Parse(modelTexts[fileName]);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    jsonErrors.Add(fileName, ex);
                }
            }

            if (jsonErrors.Count > 0)
            {
                Log.Error("Errors parsing models.");
                foreach (string fileName in jsonErrors.Keys)
                {
                    Log.Error($"{fileName}: {jsonErrors[fileName].Message}");
                }
            }

            return jsonErrors.Count == 0;
        }

        private static async Task<IReadOnlyDictionary<Dtmi, DTEntityInfo>> ParseModelsAsync(Dictionary<string, string> modelTexts)
        {
            IReadOnlyDictionary<Dtmi, DTEntityInfo> entities = null;
            try
            {
                var parser = new ModelParser(new ParsingOptions() { AllowUndefinedExtensions = true });
                entities = await parser.ParseAsync(modelTexts.Values.ToAsyncEnumerable());
            }
            catch (ParsingException ex)
            {
                Log.Error("Errors parsing models.");
                foreach (ParsingError error in ex.Errors)
                {
                    Log.Error(error.Message);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Errors parsing models.");
                Log.Error(ex.Message);
            }

            return entities;
        }

        private static IEnumerable<DTInterfaceInfo> OrderInterfaces(IEnumerable<DTInterfaceInfo> interfaces)
        {
            // This function sorts interfaces from interfaces with no dependencies to interfaces with dependencies.
            // Using a depth-first search and post-order processing, we get an ordering from no dependencies to most dependencies.

            // Build the set of all referenced interfaces.
            HashSet<DTInterfaceInfo> referencedInterfaces = new HashSet<DTInterfaceInfo>(new DTInterfaceInfoEqualityComparer());
            foreach (DTInterfaceInfo @interface in interfaces)
            {
                foreach (DTInterfaceInfo referencedInterface in @interface.Extends)
                {
                    referencedInterfaces.Add(referencedInterface);
                }

                IEnumerable<DTInterfaceInfo> componentSchemas = from content in @interface.Contents.Values
                                                                where content.EntityKind == DTEntityKind.Component
                                                                select ((DTComponentInfo)content).Schema;
                foreach (DTInterfaceInfo referencedInterface in componentSchemas)
                {
                    referencedInterfaces.Add(referencedInterface);
                }
            }

            // The roots of the trees are all the interfaces that are not referenced.
            IEnumerable<DTInterfaceInfo> rootInterfaces = interfaces.Except(referencedInterfaces, new DTInterfaceInfoEqualityComparer());

            // For each root, perform depth-first, post-order processing to produce a sorted tree.
            OrderedHashSet<DTInterfaceInfo> orderedInterfaces = new OrderedHashSet<DTInterfaceInfo>(new DTInterfaceInfoEqualityComparer());
            foreach (DTInterfaceInfo rootInterface in rootInterfaces)
            {
                OrderedHashSet<DTInterfaceInfo> rootInterfaceOrderedInterfaces = new OrderedHashSet<DTInterfaceInfo>(new DTInterfaceInfoEqualityComparer());
                OrderInterface(rootInterfaceOrderedInterfaces, rootInterface);
                foreach (DTInterfaceInfo rootInterfaceOrderedInterface in rootInterfaceOrderedInterfaces)
                {
                    if (!orderedInterfaces.Contains(rootInterfaceOrderedInterface))
                    {
                        orderedInterfaces.Add(rootInterfaceOrderedInterface);
                    }
                }
            }

            return orderedInterfaces;
        }

        private static void OrderInterface(OrderedHashSet<DTInterfaceInfo> orderedInterfaces, DTInterfaceInfo @interface)
        {
            // Order each extended interface.
            foreach (DTInterfaceInfo extendedInterface in @interface.Extends)
            {
                OrderInterface(orderedInterfaces, extendedInterface);
            }

            // Order each component schema interface.
            IEnumerable<DTInterfaceInfo> componentSchemas = from content in @interface.Contents.Values
                                                            where content.EntityKind == DTEntityKind.Component
                                                            select ((DTComponentInfo)content).Schema;
            foreach (DTInterfaceInfo componentSchemaInterface in componentSchemas)
            {
                OrderInterface(orderedInterfaces, componentSchemaInterface);
            }

            // Add this interface to the list of ordered interfaces.
            if (!orderedInterfaces.Contains(@interface))
            {
                orderedInterfaces.Add(@interface);
            }
        }

        private static IEnumerable<string> GetFileNames(IEnumerable<string> fileSpecs)
        {
            IEnumerable<string> fileNames = Enumerable.Empty<string>();
            foreach (string fileSpec in fileSpecs)
            {
                fileNames = fileNames.Concat(Glob.Expand(fileSpec).Select(fsi => fsi.FullName));
            }

            return fileNames;
        }
    }
}
