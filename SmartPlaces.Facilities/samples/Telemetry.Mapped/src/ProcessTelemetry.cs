//-----------------------------------------------------------------------
// <copyright file="ProcessTelemetry.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Messaging.EventHubs.Consumer;
    using Azure.Messaging.EventHubs.Primitives;
    using Azure.Messaging.EventHubs.Processor;
    using Azure.Storage.Blobs;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Telemetry.Processors;
    using Telemetry.Interfaces;

    internal class ProcessTelemetry : BackgroundService
    {
        private readonly ILogger<ProcessTelemetry> logger;
        private readonly IConfiguration configuration;
        private readonly ITelemetryIngestionProcessor telemetryIngestionProcessor;
        private readonly TelemetryClient telemetryClient;

        private readonly string eventHubNamespaceConnectionString;
        private readonly string eventHubName;
        private readonly string storageAccountEndpoint;
        private readonly string blobContainerName;

        private EventHubProcessor<EventProcessorPartition>? processor;

        /// <summary>
        /// Setup for what to do when the process is running.
        /// </summary>
        /// <param name="logger">A way to communicate.</param>
        /// <param name="configuration">Any settings, environment variables, command arguments, etc. that have been passed to the process.</param>
        /// <param name="telemetryIngestionProcessor">An instantiation of what to do with the telemetry.</param>
        /// <param name="telemetryClient">A way to log metrics.</param>
        public ProcessTelemetry(ILogger<ProcessTelemetry> logger, IConfiguration configuration, ITelemetryIngestionProcessor telemetryIngestionProcessor, TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.telemetryClient = telemetryClient;
            this.telemetryIngestionProcessor = telemetryIngestionProcessor;

            eventHubName = configuration["EventHubName"];
            eventHubNamespaceConnectionString = configuration[$"{eventHubName}-PrimaryConnectionString"];
            storageAccountEndpoint = configuration["StorageAccountEndpoint"];
            blobContainerName = $"checkpoint{eventHubName}";

            logger.LogInformation("AzureDigitalTwinsEndpoint: {AzureDigitalTwinsEndpoint}", configuration["AzureDigitalTwinsEndpoint"]);
            logger.LogInformation("StorageAccountEndpoint: {StorageAccountEndpoint}", storageAccountEndpoint);
            logger.LogInformation("KeyVaultEndpoint: {KeyVaultEndpoint}", configuration["KeyVaultEndpoint"]);
        }

        /// <summary>
        /// Defines what to to during the life of the process.
        /// </summary>
        /// <param name="cancellationToken">A way to stop things.</param>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Create a blob container client that the event processor will use.
            // Get a reference to the upload container and then create it if it does not exist.
            logger.LogInformation("Configuring EventHub and Checkpoint");
            Uri containerUri = new Uri($"{storageAccountEndpoint}/{blobContainerName}");
            BlobContainerClient containerClient = new BlobContainerClient(containerUri, new DefaultAzureCredential());
            containerClient.CreateIfNotExists(cancellationToken: cancellationToken);

            // Setup listener to EventHub and define what to do with each message
            // in the batch
            processor = new EventHubProcessor<EventProcessorPartition>( logger,
                                                                        async (eventData, cancellationToken) => await telemetryIngestionProcessor.IngestFromEventHubAsync(eventData, cancellationToken),
                                                                        configuration.GetValue<int>("EventHubBatchMaximumCount"),
                                                                        EventHubConsumerClient.DefaultConsumerGroupName,
                                                                        eventHubNamespaceConnectionString,
                                                                        eventHubName,
                                                                        containerClient,
                                                                        new EventProcessorOptions()
                                                                        {
                                                                            // Selecting Greedy because our current design is to have only
                                                                            // one iotHub device which translates to only one eventHub partition.
                                                                            // Multiple service consumer instances cant share a partition.
                                                                            LoadBalancingStrategy = LoadBalancingStrategy.Greedy,
                                                                            PrefetchCount = configuration.GetValue<int>("EventHubPrefetchCount"),
                                                                            MaximumWaitTime = TimeSpan.FromSeconds(configuration.GetValue<int>("EventHubMaxTimeWithoutBatchInSeconds")),
                                                                        });

            // Start the processing
            logger.LogInformation("Starting to process telemetry...");
            await processor.StartProcessingAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Defines what actions need to be taken when the process is
        /// asked to stop.
        /// </summary>
        /// <param name="cancellationToken">A way to stop things.</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop the processing
            logger.LogInformation("Shutting down Telemetry Service", nameof(ITelemetryIngestionProcessor));
            if (processor != null)
            {
                await processor.StopProcessingAsync(cancellationToken);
            }

            // Make sure all logs and metrics have been sent
            await telemetryClient.FlushAsync(CancellationToken.None);

            await base.StopAsync(cancellationToken);
        }
    }
}