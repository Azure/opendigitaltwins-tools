// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;

    /// <summary>
    /// Defines the methods to be implemented by an OntologyMappingManager
    /// </summary>
    public interface IOntologyMappingManager
    {
        /// <summary>
        /// Validates that all Output DTMIs listed in the Interface Remaps exist in the target object model
        /// </summary>
        /// <param name="targetObjectModel">A dictionary of DTMI to DTEntityInfo mappings which are valid in the target ontology</param>
        /// <param name="invalidTargets">A list of invalid output mappings in the InterfaceRemaps</param>
        /// <returns>true if all targets are valid, false otherwise</returns>
        public bool ValidateTargetOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> targetObjectModel, out List<string> invalidTargets);

        /// <summary>
        /// For a given DTMI from the source ontology, get the DTMI for the target ontology
        /// </summary>
        /// <param name="inputDtmi">The DTMI from the source ontology</param>
        /// <param name="dtmiRemap">The Remap Entity if there is one</param>
        /// <returns>true if a remap exists, false otherwise</returns>
        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap);

        /// <summary>
        /// For a given relationship name in the input ontology, get the name of the relationship in the target ontology
        /// </summary>
        /// <param name="inputRelationship">The name of the relationship in the input ontology. i.e. "hasPart"</param>
        /// <param name="outputRelationship">The name of the relationship in the output ontology. i.e. "isLocationOf"</param>
        /// <returns></returns>
        public bool TryGetRelationshipRemap(string inputRelationship, out RelationshipRemap? outputRelationship);

        /// <summary>
        /// In some cases, the contents of one input property may need to be copied to multiple other fields in the target ontology. For instance, if 
        /// the target ontology requires that the name field always be populated, but the source name field may be null and the description field be more reliable,
        /// a chain of fields can be set here so that there is a priority list of fields that will backfill the name field if the input name field is null.
        /// </summary>
        /// <param name="outputDtmiFilter">A regex which describes which output dtmi's this rule applies to</param>
        /// <param name="outputPropertyName">The target property name</param>
        /// <param name="inputPropertyNames">A space-delimited, ordered, list of fields which declare the backfill precedence</param>
        /// <returns>true if a mapping exists, false otherwise</returns>
        public bool TryGetFillProperty(string outputDtmiFilter, string outputPropertyName, out FillProperty? fillProperty);

        /// <summary>
        /// In some cases, a property of the input model needs to be put into a different field or collection in the target model. A declaration can be made to map the input field to the appropriate output field
        /// </summary>
        /// <param name="outputDtmiFilter">A regex which describes which output dtmi's this rule applies to</param>
        /// <param name="outputPropertyName">The name of the output property</param>
        /// <param name="inputProperty">The property projection for the output property</param>
        /// <returns>true if a mapping exists, false otherwise</returns>
        public bool TryGetPropertyProjection(string outputDtmiFilter, string outputPropertyName, out PropertyProjection? inputProperty);
    }
}
