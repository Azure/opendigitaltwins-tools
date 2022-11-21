//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry
{
    using System;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Telemetry.Interfaces;
    using Telemetry.Processors;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Telemetry Processing");

            using IHost host = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var settings = config.Build();

                    var vaultUri = settings["KeyVaultEndpoint"];

                    if (string.IsNullOrWhiteSpace(vaultUri))
                    {
                        Console.WriteLine("Startup Variable 'KeyVaultEndpoint' not found. Exiting");
                        throw new ArgumentNullException("KeyVaultEndpoint");
                    }

                    config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging();

                    services.AddApplicationInsightsTelemetryWorkerService(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["AppInsightsConnectionString"];
                        options.EnableAdaptiveSampling = false;
                    });

                    // Implements ITwinMappingIndexer, IOutputGraphManager
                    services.AddIngestionManager<IngestionManagerOptions>(options =>
                    {
                        options.AzureDigitalTwinsEndpoint = hostContext.Configuration["AzureDigitalTwinsEndpoint"];
                    });

                    // Ties Topology and Telemetry together
                    // Implements IDistributedCache
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration["RedisCacheConnectionString"];
                    });
                    services.AddSingleton<ITwinMappingIndexer, RedisTwinMappingIndexer>();
                    services.AddSingleton<ITelemetryIngestionProcessor, TelemetryIngestionProcessor<IngestionManagerOptions>>();

                    services.AddHostedService<ProcessTelemetry>();

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Console.WriteLine("Hi Devs");
                    }
                })
                .Build();

            // Start the host
            await host.RunAsync();

            Console.WriteLine("Completed Telemetry Processing");
        }
    }
}
