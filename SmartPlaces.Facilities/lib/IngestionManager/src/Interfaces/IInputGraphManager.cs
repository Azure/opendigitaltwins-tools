//-----------------------------------------------------------------------
// <copyright file="IInputGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IngestionManager.Interfaces
{
    using System.Text.Json;
    using System.Threading.Tasks;

    public interface IInputGraphManager
    {
        public bool TryGetDtmi(string interfaceType, out string dtmi);

        public Task<JsonDocument?> GetTwinGraphAsync(string query);

        public string GetOrganizationQuery();

        public string GetBuildingsForSiteQuery(string siteId);

        public string GetFloorQuery(string basicDtId);

        public string GetBuildingThingsQuery(string buildingDtId);

        public string GetPointsForThingQuery(string thingDtId);
    }
}
