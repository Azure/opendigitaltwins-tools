namespace DTDLValidator.Interactive
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    [Verb("load", HelpText = "Load models.")]
    internal class LoadCommand
    {
        [Value(0, HelpText = "List of file names to load.")]
        public IEnumerable<string> FileNames { get; set; }

        public async Task Run(Interactive p)
        {
            List<string> modelTexts = new List<string>();
            foreach (string fileName in FileNames)
            {
                string directoryName = Path.GetDirectoryName(fileName);
                if (string.IsNullOrWhiteSpace(directoryName))
                {
                    directoryName = ".";
                }

                string[] expandedFileNames = Directory.GetFiles(directoryName, Path.GetFileName(fileName));
                foreach (string expandedFileName in expandedFileNames)
                {
                    modelTexts.Add(File.ReadAllText(expandedFileName));
                    Console.WriteLine($"Loaded {expandedFileName}");
                }
            }

            // Parse the models.
            // The set of entities returned from ParseAsync includes entities loaded by the resolver.
            Console.WriteLine();
            try
            {
                (IReadOnlyDictionary<Dtmi, DTEntityInfo> entities, IEnumerable<DTInterfaceInfo> resolvedInterfaces) = await p.DTDLParser.ParseAsync(modelTexts);
                foreach (Dtmi entityDtmi in entities.Keys)
                {
                    Log.Ok($"Parsed {entityDtmi.AbsoluteUri}");
                }

                // Store only the newly loaded interfaces.
                // Because the entities returned from ParseAsync contains
                // more than just interfaces and also any entities loaded by the resolver:
                // - Filter to just interfaces
                // - Exclude interfaces that were loaded by the resolver.
                // The above seems reasonable for a client to do, since the parser
                // doesn't/shouldn't know these details.
                Console.WriteLine();
                IEnumerable<DTInterfaceInfo> interfaces = from entity in entities.Values
                                                          where entity.EntityKind == DTEntityKind.Interface
                                                          select entity as DTInterfaceInfo;
                interfaces = interfaces.Except(resolvedInterfaces, new DTInterfaceInfoComparer());
                foreach (DTInterfaceInfo @interface in interfaces)
                {
                    p.Models.Add(@interface.Id, @interface);
                    Console.WriteLine($"Stored {@interface.Id.AbsoluteUri}");
                }
            }
            catch (ParsingException pe)
            {
                Log.Error($"*** Error parsing models");
                int derrcount = 1;
                foreach (ParsingError err in pe.Errors)
                {
                    Log.Error($"Error {derrcount}:");
                    Log.Error($"{err.Message}");
                    Log.Error($"Primary ID: {err.PrimaryID}");
                    Log.Error($"Secondary ID: {err.SecondaryID}");
                    Log.Error($"Property: {err.Property}\n");
                    derrcount++;
                }
            }
        }

        private class DTInterfaceInfoComparer : IEqualityComparer<DTInterfaceInfo>
        {
            public bool Equals([AllowNull] DTInterfaceInfo x, [AllowNull] DTInterfaceInfo y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                {
                    return false;
                }

                return x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] DTInterfaceInfo obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}
