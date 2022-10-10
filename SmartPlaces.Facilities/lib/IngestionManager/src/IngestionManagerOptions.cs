// -----------------------------------------------------------------------
// <copyright file="IngestionManagerOptions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Configures the connection to the Azure Digital Twins Instance for Topology Ingestion.
    /// </summary>
    public class IngestionManagerOptions
    {
        /// <summary>
        /// Gets or sets the Url of the Azure Digital Twins instance to connect to.
        /// </summary>
        [Required]
        [RegularExpression(@"^(https:\/\/)(.*)\.api\..*\.digitaltwins\.[a-zA-Z\.]*(\/?)$")]
        public string AzureDigitalTwinsEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of times to retry create twin attempts per twin/relationship.
        /// Defaults to 3.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay in milliseconds between retry create twin attempts per twin/relationship.
        /// Defaults to 50ms.
        /// </summary>
        public int RetryDelayInMs { get; set; } = 50;

        /// <summary>
        /// Gets or sets the resource to be used when generating a Token for accessing Azure Digital Twins.
        /// This needs to be changed when working with non-public clouds.
        /// Defaults to https://digitaltwins.azure.net/.default.
        /// </summary>
        public string AdtResource { get; set; } = "https://digitaltwins.azure.net/.default";

        /// <summary>
        /// Gets or sets the maximum number parallel threads to use when uploading to Azure Digital Twins.
        /// Defaults to 10 threads.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 10;
    }
}
