namespace DTDLValidator
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;

    class Program
    {
        public class Options
        {
            [Option('e', "extension", Default = "json", SetName = "normal", HelpText = "File extension of files to be processed.")]
            public string Extension { get; set; }

            [Option('d', "directory", SetName = "normal", HelpText = "Directory to search files in.")]
            public string Directory { get; set; }

            [Option('r', "recursive", Default = false, SetName = "normal", HelpText = "Search given directory (option -d) only (false) or subdirectories too (true)")]
            public bool Recursive { get; set; }

            [Option('f', "files", HelpText = "Input files to be processed. If -d option is also specified, these files are read in addition.")]
            public IEnumerable<string> InputFiles { get; set; }

            [Option('i', "interactive", Default = false, SetName = "interactive", HelpText = "Run in interactive mode")]
            public bool Interactive { get; set; }
        }

        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void RunOptions(Options opts)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string dtdlParserVersion = "<unknown>";
            foreach (Assembly a in assemblies)
            {
                if (a.GetName().Name.EndsWith("DigitalTwins.Parser"))
                    dtdlParserVersion = a.GetName().Version.ToString();
            }

            Log.Ok($"Simple DTDL Validator (dtdl parser library version {dtdlParserVersion})");

            if (opts.Interactive == true)
            {
                Log.Alert("Entering interactive mode");
                Interactive.Interactive i = new Interactive.Interactive();
                return;
            }

            DirectoryInfo dinfo = null;
            try
            {
                if(opts.Directory != null && opts.Directory != string.Empty)
                {
                    dinfo = new DirectoryInfo(opts.Directory);
                    Log.Alert($"Validating *.{opts.Extension} files in folder '{dinfo.FullName}'.\nRecursive is set to {opts.Recursive}\n");

                    if (dinfo.Exists == false)
                    {
                        Log.Error($"Specified directory '{opts.Directory}' does not exist: Exiting...");
                        throw new Exception();
                    }
                }

            } catch (Exception e)
            {
                Log.Error($"Error accessing the target directory '{opts.Directory}': \n{e.Message}");
                Environment.Exit(0);
            }

            SearchOption searchOpt = opts.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = opts.InputFiles.ToList();
            
            if(dinfo!= null)
            {
                dinfo.EnumerateFiles($"*.{opts.Extension}", searchOpt).ToList().ForEach(file => files.Add(file.FullName));
            }
            
            if (files.Count() == 0)
            {
                Log.Alert("No matching files found. Exiting.");
                return;
            }
            
            var modelDict = new Dictionary<string, string>();
            int count = 0;
            string lastFile = "<none>";
            try
            {
                foreach (var file in files)
                {
                    StreamReader r = new StreamReader(file);
                    string dtdl = r.ReadToEnd(); 
                    r.Close();
                    modelDict.Add(file, dtdl);
                    lastFile = file;
                    count++;
                }
            } catch (Exception e)
            {
                Log.Error($"Could not read files. \nLast file read: {lastFile}\nError: \n{e.Message}");
                Environment.Exit(0);
            }
            
            Log.Ok($"Read {count} files from specified directory");
            int errJson = 0;
            foreach (string file in modelDict.Keys)
            {
                modelDict.TryGetValue(file, out string dtdl);
                try
                {
                    JsonDocument.Parse(dtdl);
                } catch (Exception e)
                {
                    Log.Error($"Invalid json found in file {file}.\nJson parser error \n{e.Message}");
                    errJson++;
                }
            }
            
            if (errJson>0)
            {
                Log.Error($"\nFound  {errJson} Json parsing errors");
                Environment.Exit(0);
            }
            
            Log.Ok($"Validated JSON for all files - now validating DTDL");
            List<string> modelList = modelDict.Values.ToList<string>();
            ModelParser parser = new ModelParser();
            parser.DtmiResolver = new DtmiResolver(Resolver);
            try
            {
                IReadOnlyDictionary<Dtmi, DTEntityInfo> om = parser.Parse(modelList);
                Log.Out("");
                Log.Ok($"**********************************************");
                Log.Ok($"** Validated all files - Your DTDL is valid **");
                Log.Ok($"**********************************************");
                Log.Out($"Found a total of {om.Keys.Count()} entities");
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
            
                Environment.Exit(0);
            }
            catch (ResolutionException ex)
            {
                Log.Error(ex, "Could not resolve required references");
                Environment.Exit(0);
            }
        }

        static IEnumerable<string> Resolver(IReadOnlyCollection<Dtmi> dtmis)
        {
            Log.Error($"*** Error parsing models. Missing:");
            foreach (Dtmi d in dtmis)
            {
                Log.Error($"  {d}");
            }

            return null;
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            Log.Error($"Invalid command line.");
            foreach (Error e in errs)
            {
                Log.Error($"{e.Tag}: {e}");
            }            
        }
    }
}
