//-----------------------------------------------------------------------
// <copyright file="ITwinMappingIndexer.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    using System.Threading.Tasks;

    /// <summary>
    /// Methods for working with a cache store of source IDs to Digital Twin Ids.
    /// </summary>
    public interface ITwinMappingIndexer
    {
        /// <summary>
        /// Add or update a mapping to the cache, in the latter case overwriting any existing mapping.
        /// </summary>
        /// <param name="sourceId">The source device ID from the source graph.</param>
        /// <param name="twinMap">The target twin map (wrapping ID + model) for the target graph.</param>
        /// <returns>An awaitable task.</returns>
        public Task UpsertTwinIndexAsync(string sourceId, TwinMap twinMap);

        /// <summary>
        /// Get a mapping from the cache for a passed in sourceId, if it exists.
        /// </summary>
        /// <param name="sourceId">The source device key from the source graph.</param>
        /// <returns>The twin map (wrapping ID + model) for the target graph if it exists in the cache, else null.</returns>
        public Task<TwinMap?> GetTwinIndexAsync(string sourceId);
    }
}
