// -----------------------------------------------------------------------
// <copyright file="IOntologyMappingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using DTDLParser;
    using DTDLParser.Models;

    /// <summary>
    /// Defines the methods to be implemented by an OntologyMappingManager.
    /// </summary>
    public interface IOntologyMappingManager
    {
        /// <summary>
        /// Validates that all Output DTMIs listed in the Interface Remaps exist in the target object model.
        /// </summary>
        /// <param name="targetObjectModel">A dictionary of DTMI to DTEntityInfo mappings which are valid in the target ontology.</param>
        /// <param name="invalidTargets">A list of invalid output mappings in the InterfaceRemaps.</param>
        /// <returns>true if all targets are valid, false otherwise.</returns>
        public bool ValidateTargetOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> targetObjectModel, out List<string> invalidTargets);

        /// <summary>
        /// Validates that all Inputput DTMIs listed in the Interface Remaps exist in the source object model.
        /// </summary>
        /// <param name="sourceObjectModel">A dictionary of DTMI to DTEntityInfo mappings which are valid in the source ontology.</param>
        /// <param name="invalidSources">A list of invalid input mappings in the InterfaceRemaps.</param>
        /// <returns>true if all sources are valid, false otherwise.</returns>
        public bool ValidateSourceOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> sourceObjectModel, out List<string> invalidSources);

        /// <summary>
        /// For a given DTMI from the source ontology, get the DTMI for the target ontology.
        /// </summary>
        /// <param name="inputDtmi">The DTMI from the source ontology.</param>
        /// <param name="dtmiRemap">The remap entity for the input DTMI if there is one.</param>
        /// <returns>true if a remap exists, false otherwise.</returns>
        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap);

        /// <summary>
        /// For a given relationship name in the input ontology, get the name of the relationship in the target ontology.
        /// </summary>
        /// <param name="inputRelationship">The name of the relationship in the input ontology. i.e. "hasPart".</param>
        /// <param name="relationshipRemap">The remap entity for the input relationship if there is one.</param>
        /// <returns><c>true</c> if a matching output relationship could be found, else <c>false</c>.</returns>
        public bool TryGetRelationshipRemap(string inputRelationship, out RelationshipRemap? relationshipRemap);

        /// <summary>
        /// In some cases, the contents of one input property may need to be copied to multiple other fields in the target ontology. For instance, if
        /// the target ontology requires that the name field always be populated, but the source name field may be null and the description field be more reliable,
        /// a chain of fields can be set here so that there is a priority list of fields that will backfill the name field if the input name field is null.
        /// </summary>
        /// <param name="outputDtmiFilter">A regex which describes which output dtmi's this rule applies to.</param>
        /// <param name="outputPropertyName">The target property name.</param>
        /// <param name="fillProperty">The fill property entity for the output property if there is one.</param>
        /// <returns>true if a mapping exists, false otherwise.</returns>
        public bool TryGetFillProperty(string outputDtmiFilter, string outputPropertyName, out FillProperty? fillProperty);

        /// <summary>
        /// In some cases, a property of the input model needs to be put into a different field or collection in the target model. A declaration can be made to map the input field to the appropriate output field.
        /// </summary>
        /// <param name="outputDtmiFilter">A regex which describes which output dtmi's this rule applies to.</param>
        /// <param name="outputPropertyName">The name of the output property.</param>
        /// <param name="propertyProjection">The property projection for the output property if there is one.</param>
        /// <returns>true if a mapping exists, false otherwise.</returns>
        public bool TryGetPropertyProjection(string outputDtmiFilter, string outputPropertyName, out PropertyProjection? propertyProjection);

        /// <summary>
        /// In some cases, a property of the input model contains an object where one of the properties of that object needs to be put into a different field in the target model. A declaration can be made to map the input field to the appropriate output field.
        /// </summary>
        /// <param name="outputDtmiFilter">A regex which describes which output dtmi's this rule applies to.</param>
        /// <param name="outputPropertyName">The name of the output property.</param>
        /// <param name="objectTransformation">The objectTransformation for the output property if there is one.</param>
        /// <returns>
        /// NoPropertyMatch (There is no match on the property passed in at all. No use testing further by type).
        /// PropertyMatchOnly (There is a match on the property name, but not on the type. Try ancestors.
        /// PropertyAndTypeMatch There is a match on the property name and the type. We have a match.
        /// </returns>
        public ObjectTransformationMatch TryGetObjectTransformation(string outputDtmiFilter, string outputPropertyName, out ObjectTransformation? objectTransformation);
    }
}
