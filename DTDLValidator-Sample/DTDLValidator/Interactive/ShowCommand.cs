using CommandLine;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Threading.Tasks;

namespace DTDLValidator.Interactive
{
    [Verb("show", HelpText = "Display model definition.")]
    internal class ShowCommand
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
            
                if (p.Models.TryGetValue(modelId, out DTInterfaceInfo @interface))
                {
                    Console.WriteLine(@interface.GetJsonLdText());
                }
            }
            catch (Exception e)
            {
                Log.Error($"{ModelId} is not a valid dtmi");
            }

            return Task.FromResult<object>(null);
        }
    }
}
