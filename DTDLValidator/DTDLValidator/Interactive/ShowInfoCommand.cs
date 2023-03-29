namespace DTDLValidator.Interactive
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Verb("showinfo", HelpText = "Display parent interfaces, properties and relationships defined in a model, taking inheritance into account")]
    internal class ShowInfoCommand
    {
        [Value(0, HelpText = "Model id to show.")]
        public string ModelId { get; set; }

        public Task Run(Interactive p)
        {
            if (ModelId == null)
            {
                Log.Error("Please specify a valid model id");
                return Task.FromResult<object>(null);
            }

            try
            {
                Dtmi modelId = new Dtmi(ModelId);
            
                if (p.Models.TryGetValue(modelId, out DTInterfaceInfo dti))
                {
                    Log.Ok("Inherited interfaces:");
                    foreach (DTInterfaceInfo parent in dti.Extends)
                    {
                        Log.Ok($"    {parent.Id}");
                    }

                    IReadOnlyDictionary<string, DTContentInfo> contents = dti.Contents;
                    Log.Alert($"  Properties:");
                    var props = contents
                                    .Where(p => p.Value.EntityKind == DTEntityKind.Property)
                                    .Select(p => p.Value);
                    foreach (DTPropertyInfo pi in props)
                    {
                        pi.Schema.DisplayName.TryGetValue("en", out string displayName);
                        Log.Out($"    {pi.Name}: {displayName ?? pi.Schema.ToString()}");
                    }

                    Log.Out($"  Relationships:", ConsoleColor.DarkMagenta);
                    var rels = contents
                                    .Where(p => p.Value.EntityKind == DTEntityKind.Relationship)
                                    .Select(p => p.Value);
                    foreach (DTRelationshipInfo ri in rels)
                    {
                        string target = "<any_type>";
                        if (ri.Target != null)
                            target = ri.Target.ToString();
                        Log.Out($"    {ri.Name} -> {target}");
                    }
                }
            }
            catch (Exception)
            {
                Log.Error($"{ModelId} is not a valid dtmi");
            }

            return Task.FromResult<object>(null);
        }
    }
}
