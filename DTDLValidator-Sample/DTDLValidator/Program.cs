using CommandLine;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace DTDLValidator
{
    class Program
    {
        public class Options
        {
            [Option('e', "extension", Default = "json", SetName = "normal", HelpText = "File extension of files to be processed.")]
            public string Extension { get; set; }

            [Option('d', "directory", Default = ".", SetName = "normal", HelpText = "Directory to search files in.")]
            public string Directory { get; set; }

            [Option('r', "recursive", Default = true, SetName = "normal", HelpText = "Search given directory (option -d) only (false) or subdirectories too (true)")]
            public bool Recursive { get; set; }

            //[Option('f', "files", HelpText = "Input files to be processed. If -d option is also specified, these files are read in addition.")]
            //public IEnumerable<string> InputFiles { get; set; }

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
                dinfo = new DirectoryInfo(opts.Directory);
            } catch (Exception e)
            {
                Log.Error($"Error accessing the target directory '{opts.Directory}': \n{e.Message}");
                return;
            }
            Log.Alert($"Validating *.{opts.Extension} files in folder '{dinfo.FullName}'.\nRecursive is set to {opts.Recursive}\n");
            if (dinfo.Exists == false)
            {
                Log.Error($"Specified directory '{opts.Directory}' does not exist: Exiting...");
                return;
            }
            else
            {
                SearchOption searchOpt = opts.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = dinfo.EnumerateFiles($"*.{opts.Extension}", searchOpt);
                if (files.Count() == 0)
                {
                    Log.Alert("No matching files found. Exiting.");
                    return;
                }
                Dictionary<FileInfo, string> modelDict = new Dictionary<FileInfo, string>();
                int count = 0;
                string lastFile = "<none>";
                try
                {
                    foreach (FileInfo fi in files)
                    {
                        StreamReader r = new StreamReader(fi.FullName);
                        string dtdl = r.ReadToEnd(); r.Close();
                        modelDict.Add(fi, dtdl);
                        lastFile = fi.FullName;
                        count++;
                    }
                } catch (Exception e)
                {
                    Log.Error($"Could not read files. \nLast file read: {lastFile}\nError: \n{e.Message}");
                    return;
                }
                Log.Ok($"Read {count} files from specified directory");
                int errJson = 0;
                foreach (FileInfo fi in modelDict.Keys)
                {
                    modelDict.TryGetValue(fi, out string dtdl);
                    try
                    {
                        JsonDocument.Parse(dtdl);
                    } catch (Exception e)
                    {
                        Log.Error($"Invalid json found in file {fi.FullName}.\nJson parser error \n{e.Message}");
                        errJson++;
                    }
                }
                if (errJson>0)
                {
                    Log.Error($"\nFound  {errJson} Json parsing errors");
                    return;
                }
                Log.Ok($"Validated JSON for all files - now validating DTDL");
                List<string> modelList = modelDict.Values.ToList<string>();
                ModelParser parser = new ModelParser();
                parser.DtmiResolver = Resolver;
                try
                {
                    IReadOnlyDictionary<Dtmi, DTEntityInfo> om = parser.ParseAsync(modelList).GetAwaiter().GetResult();
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
                    return;
                }
                catch (ResolutionException rex)
                {
                    Log.Error("Could not resolve required references");
                }
            } 
        }

        static async Task<IEnumerable<string>> Resolver(IReadOnlyCollection<Dtmi> dtmis)
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
                Log.Error($"{e.Tag}: {e.ToString()}");
            }
            
        }
    }
}
