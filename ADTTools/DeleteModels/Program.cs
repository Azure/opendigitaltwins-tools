using ADTToolsLibrary;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using CommandLine;
using System;
using System.Linq;
using System.Net;

namespace DeleteModels
{
    class Program
    {
        private readonly Options options;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => new Program(options).Run());
        }

        private Program(Options options)
        {
            this.options = options;
        }

        private void Run()
        {
            try
            {
                var credential = new InteractiveBrowserCredential(options.TenantId, options.ClientId);
                var client = new DigitalTwinsClient(new UriBuilder("https", options.HostName).Uri, credential);
                DeleteAllModels(client, 1);
            }
            catch (Exception ex)
            {
                Log.Error($"Deleting models failed.");
                Log.Error(ex.Message);
            }
        }

        private void DeleteAllModels(DigitalTwinsClient client, int iteration)
        {
            foreach (DigitalTwinsModelData modelData in client.GetModels())
            {
                try
                {
                    client.DeleteModel(modelData.Id);
                    Log.Ok($"Deleted model '{modelData.Id}' (Iteration {iteration})");
                }
                catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Conflict)
                {
                    // This model is a dependent and will be deleted in the next iteration.
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to delete model '{modelData.Id}': {ex.Message}");
                }
            }

            if (client.GetModels().Any())
            {
                DeleteAllModels(client, iteration + 1);
            }
        }
    }
}
