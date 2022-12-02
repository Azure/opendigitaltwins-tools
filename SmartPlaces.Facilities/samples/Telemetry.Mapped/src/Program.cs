//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry
{
    using System;
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
        /// <summary>
        /// Entry point into Telemetry.
        /// </summary>
        /// <param name="args">An option for passing in configuration values, though not required.</param>
        /// <exception cref="ArgumentNullException">Thrown when required configuration values are not present.</exception>
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
                    // Configure ILogger
                    services.AddLogging();

                    // Configure options for ApplicationInsights
                    services.AddApplicationInsightsTelemetryWorkerService(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["AppInsightsConnectionString"];
                        options.EnableAdaptiveSampling = false;
                    });

                    // This is for getting the TwinId and TwinModelId 
                    // based on the MappingKey which is provided by Mapped.
                    // Implements IOutputGraphManager
                    services.AddIngestionManager<IngestionManagerOptions>(options =>
                    {
                        options.AzureDigitalTwinsEndpoint = hostContext.Configuration["AzureDigitalTwinsEndpoint"];
                    });

                    // Ties Topology and Telemetry together setting up 
                    // Communication with the Cloud Redis
                    // Implements IDistributedCache
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration["RedisCacheConnectionString"];
                    });
                    services.AddSingleton<ITwinMappingIndexer, RedisTwinMappingIndexer>();

                    // Add the processor for the main worker body
                    services.AddSingleton<ITelemetryIngestionProcessor, TelemetryIngestionProcessor<IngestionManagerOptions>>();

                    // Add the main worker body
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
