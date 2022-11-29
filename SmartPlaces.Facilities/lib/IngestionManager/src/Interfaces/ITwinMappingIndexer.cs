//-----------------------------------------------------------------------
// <copyright file="ITwinMappingIndexer.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    using System.Threading.Tasks;

    public interface ITwinMappingIndexer
    {
        public Task UpsertTwinIndexAsync(string sourceId, TwinMapEntry mapEntry);

        public Task<TwinMapEntry?> GetTwinIndexAsync(string sourceId);
    }
}
