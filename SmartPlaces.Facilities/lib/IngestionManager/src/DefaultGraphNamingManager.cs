// -----------------------------------------------------------------------
// <copyright file="DefaultGraphNamingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    /// <summary>
    /// Default naming for the output graph.
    /// </summary>
    public class DefaultGraphNamingManager : IGraphNamingManager
    {
        /// <summary>
        /// Gets the name of the relationship in the output graph. Note that the default version does not use the properties argument.
        /// </summary>
        /// <param name="sourceTwinId">The twin id of the source twin.</param>
        /// <param name="targetTwinId">The twin id of the target twin.</param>
        /// <param name="relationshipType">The type of the relationship.</param>
        /// <param name="properties">The propertie of the relationship to be encoded into the name.</param>
        /// <returns>A formatted string that represents the relationship.</returns>
        public string GetRelationshipName(string sourceTwinId, string targetTwinId, string relationshipType, IDictionary<string, object> properties)
        {
            return $"{sourceTwinId}-{targetTwinId}-{relationshipType}";
        }
    }
}
