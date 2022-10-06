//-----------------------------------------------------------------------
// <copyright file="ProcessTopology.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Contoso.Topology
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    public class ProcessTopology : BackgroundService
    {
        private readonly ILogger<ProcessTopology> logger;
        private readonly IConfiguration configuration;
        private readonly IGraphIngestionProcessor graphIngestionProcessor;

        public ProcessTopology(ILogger<ProcessTopology> logger,
                               IConfiguration configuration,
                               IGraphIngestionProcessor graphIngestionProcessor)
        {
            this.logger = logger;
            this.graphIngestionProcessor = graphIngestionProcessor;
            this.configuration = configuration;

            logger.LogInformation("AzureDigitalTwinsEndpoint: {AzureDigitalTwinsEndpoint}", configuration["AzureDigitalTwinsEndpoint"]);
            logger.LogInformation("StorageAccountEndpoint: {StorageAccountEndpoint}", configuration["StorageAccountEndpoint"]);
            logger.LogInformation("KeyVaultEndpoint: {KeyVaultEndpoint}", configuration["KeyVaultEndpoint"]);
            logger.LogInformation("OntologyMappingFilename: {OntologyMappingFilename}", configuration["OntologyMappingFilename"]);
            logger.LogInformation("TopologyRefreshIntervalInHours: {TopologyRefreshIntervalInHours}", configuration["TopologyRefreshIntervalInHours"]);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating twin ingestion processor");

            var timer = new PeriodicTimer(TimeSpan.FromHours(configuration.GetValue<int>("TopologyRefreshIntervalInHours")));

            do
            {
                logger.LogInformation("Starting to ingest topology");

                await graphIngestionProcessor.IngestFromApiAsync(cancellationToken);

                logger.LogInformation("Topology ingestion completed");
            }
            while (await timer.WaitForNextTickAsync(cancellationToken) && !cancellationToken.IsCancellationRequested);
        }
    }
}
