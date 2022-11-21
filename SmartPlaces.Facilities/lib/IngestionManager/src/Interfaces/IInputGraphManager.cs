//-----------------------------------------------------------------------
// <copyright file="IInputGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Methods for accessing an input graph source.
    /// </summary>
    public interface IInputGraphManager
    {
        /// <summary>
        /// Get a DTMI for an interfaceType.
        /// </summary>
        /// <param name="interfaceType">The name of the interface.</param>
        /// <param name="dtmi">The found DTMI.</param>
        /// <returns><c>true</c> if the DTMi is found, otherwise <c>false</c>.</returns>
        public bool TryGetDtmi(string interfaceType, out string dtmi);

        /// <summary>
        /// Loads a twin graph from a source based on a passed in graph query.
        /// </summary>
        /// <param name="query">A well-formed graph query.</param>
        /// <returns>A JsonDocument containing the results of the query.</returns>
        public Task<JsonDocument?> GetTwinGraphAsync(string query);

        /// <summary>
        /// Gets a graph query to return an organization.
        /// </summary>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string)"/>.</returns>
        public string GetOrganizationQuery();

        /// <summary>
        /// Gets a graph query to return all the buildings on a site.
        /// </summary>
        /// <param name="siteId">Twin ID of site.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string)"/>.</returns>
        public string GetBuildingsForSiteQuery(string siteId);

        /// <summary>
        /// Gets a graph query to return all the floors for a building.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string)"/>.</returns>
        public string GetFloorQuery(string buildingId);

        /// <summary>
        /// Gets a graph query to return all the things in a building.
        /// </summary>
        /// <param name="buildingId">Twin ID of building.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string)"/>.</returns>
        public string GetBuildingThingsQuery(string buildingId);

        /// <summary>
        /// Gets a graph query to return all the BMS Points for a thing
        /// (e.g., a room, a piece of equipment, some smart furniture, or any
        /// other asset that can have data points assigned).
        /// </summary>
        /// <param name="thingId">Twin ID of thing.</param>
        /// <returns>A graph query string which can be passed to <see cref="GetTwinGraphAsync(string)"/>.</returns>
        public string GetPointsForThingQuery(string thingId);
    }
}
