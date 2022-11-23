// -----------------------------------------------------------------------
// <copyright file="IOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    /// <summary>
    /// Interface defining methods required to load a set of ontology mappings.
    /// </summary>
    public interface IOntologyMappingLoader
    {
        /// <summary>
        /// Loads a set of ontology mappings (the details of which mappings are loaded,
        /// e.g., file path on disk or URI to load from, are not passed in to this method,
        /// as they are the responsibility of any implementing classes).
        /// </summary>
        /// <returns>An OntologyMapping object holding a set of defined mappings (class mappings, relationship mappings, etc).</returns>
        OntologyMapping LoadOntologyMapping();
    }
}
