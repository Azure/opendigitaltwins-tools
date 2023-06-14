// -----------------------------------------------------------------------
// <copyright file="IGraphNamingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    /// <summary>
    /// Injectable interface to allow for custom naming entities in the output graph.
    /// </summary>
    public interface IGraphNamingManager
    {
        /// <summary>
        /// Gets the name of the relationship in the output graph.
        /// </summary>
        /// <param name="sourceTwinId">The twin id of the source twin.</param>
        /// <param name="targetTwinId">The twin id of the target twin.</param>
        /// <param name="relationshipType">The type of the relationship.</param>
        /// <param name="properties">The properties of the relationship to be encoded into the name.</param>
        /// <returns>A formatted string that represents the relationship.</returns>
        public string GetRelationshipName(string sourceTwinId, string targetTwinId, string relationshipType, IDictionary<string, object> properties);
    }
}
