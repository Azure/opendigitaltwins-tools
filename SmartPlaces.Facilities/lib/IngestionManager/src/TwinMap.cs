//-----------------------------------------------------------------------
// <copyright file="RedisTwinMappingIndexer.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    public class TwinMap
    {
        /// <summary>
        /// Gets or sets the identity of the twin that is mapped to the cache key
        /// </summary>
        public string TargetTwinId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model id of the target ontology for the twinId
        /// </summary>
        public string TargetModelId { get; set; } = string.Empty;
    }
}
