//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Topology
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped.Extensions;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper.Mapped;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Topology Processing");

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

                    // Implements IInputGraphManager, IGraphIngestionProcessor, IOutputGraphManager, ITelemetryIngestionProcessor
                    services.AddMappedIngestionManager(options =>
                    {
                        // Mapped Specific
                        options.MappedToken = hostContext.Configuration["MappedToken"];
                        options.MappedRootUrl = hostContext.Configuration["MappedRootUrl"];

                        // Ingestion Manager
                        options.AzureDigitalTwinsEndpoint = hostContext.Configuration["AzureDigitalTwinsEndpoint"];
                    });

                    // Ties Topology and Telemetry together
                    // Implements IDistributedCache
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration["RedisCacheConnectionString"];
                    });
                    services.AddSingleton<ITwinMappingIndexer, RedisTwinMappingIndexer>();

                    services.AddScoped<IOntologyMappingLoader>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<MappedOntologyMappingLoader>>();
                        return new MappedOntologyMappingLoader(logger, hostContext.Configuration["ontologyMappingFilename"]);
                    });

                    services.AddScoped<IOntologyMappingManager, OntologyMappingManager>();

                    services.AddHostedService<ProcessTopology>();

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Console.WriteLine("Hi Devs");
                    }
                })
                .Build();

            // Start the host
            await host.RunAsync();

            Console.WriteLine("Completing Topology Processing");
        }
    }
}
