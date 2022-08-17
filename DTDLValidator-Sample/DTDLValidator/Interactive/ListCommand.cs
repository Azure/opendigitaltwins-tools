namespace DTDLValidator.Interactive
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using System;
    using System.Threading.Tasks;

    [Verb("list", HelpText = "List models.")]
    internal class ListCommand
    {
        public Task Run(Interactive p)
        {
            Console.WriteLine(listFormat, "Interface Id", "Display Name");
            Console.WriteLine(listFormat, "------------", "------------");
            foreach (DTInterfaceInfo @interface in p.Models.Values)
            {
                @interface.DisplayName.TryGetValue("en", out string displayName);
                Console.WriteLine(listFormat, @interface.Id.AbsoluteUri, displayName ?? "<none>");
            }

            return Task.FromResult<object>(null);
        }

        private const string listFormat = "{0,-80}{1}";
    }
}
