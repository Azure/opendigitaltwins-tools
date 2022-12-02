//-----------------------------------------------------------------------
// <copyright file="TwinMapEntry.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    /// <summary>
    /// Twin cache map value, wrapping a twin's ID and its model ID.
    /// </summary>
    public class TwinMapEntry
    {
        /// <summary>
        /// Gets or sets the ID of the twin that is mapped to the cache key.
        /// </summary>
        public string TargetTwinId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model ID of the target ontology for the twin ID.
        /// </summary>
        public string TargetModelId { get; set; } = string.Empty;
    }
}
