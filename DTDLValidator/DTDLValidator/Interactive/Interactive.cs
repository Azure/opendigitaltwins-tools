namespace DTDLValidator.Interactive
{
    using CommandLine;
    using Microsoft.Azure.DigitalTwins.Parser;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.Azure.DigitalTwins.Parser.Models;

    class Interactive
    {
        public Interactive()
        {
            DTDLParser = new DTDLParser(Models);
            Task.WaitAll(Run());
        }

        public IDictionary<Dtmi, DTInterfaceInfo> Models { get; private set; } = new Dictionary<Dtmi, DTInterfaceInfo>();

        public DTDLParser DTDLParser { get; private set; }

        private async Task Run()
        {
            Console.WriteLine("DTDLValidator Interactive Mode");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine();
                Console.Write("> ");
                string commandLine = Console.ReadLine();
                Task commandTask = Task.FromResult<object>(null);
                Parser.Default.ParseArguments<
                    CompareCommand,
                    ListCommand,
                    LoadCommand,
                    ShowCommand,
                    ShowInfoCommand,
                    ExitCommand>(SplitArgs(commandLine))
                    .WithParsed<CompareCommand>(command => commandTask = command.Run(this))
                    .WithParsed<ListCommand>(command => commandTask = command.Run(this))
                    .WithParsed<LoadCommand>(command => commandTask = command.Run(this))
                    .WithParsed<ShowCommand>(command => commandTask = command.Run(this))
                    .WithParsed<ShowInfoCommand>(command => commandTask = command.Run(this))
                    .WithParsed<ExitCommand>(command => exit = true);
                await commandTask;
            }
        }

        private string[] SplitArgs(string arg)
        {
            int quotecount = arg.Count(x => x == '"');
            if (quotecount % 2 != 0)
            {
                Log.Alert("Your command contains an uneven number of quotes. Was that intended?");
            }

            string[] segments = arg.Split('"', StringSplitOptions.RemoveEmptyEntries);
            List<string> elements = new List<string>();
            for (int i = 0; i < segments.Length; i++)
            {
                if (i % 2 == 0)
                {
                    string[] parts = segments[i].Split(new char[] { }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ps in parts)
                        elements.Add(ps.Trim());
                }
                else
                {
                    elements.Add(segments[i].Trim());
                }
            }

            return elements.ToArray();
        }
    }
}
