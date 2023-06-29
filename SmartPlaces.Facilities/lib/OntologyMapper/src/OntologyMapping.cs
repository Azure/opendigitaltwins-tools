// -----------------------------------------------------------------------
// <copyright file="OntologyMapping.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the structure of the files stored in the Mappings folder for deserialization.
    /// </summary>
    public class OntologyMapping
    {
        /// <summary>
        /// Gets or sets information regarding the contents of the file for reference purposes.
        /// </summary>
        public MappingHeader Header { get; set; } = new MappingHeader();

        /// <summary>
        /// Gets or sets a set of mappings from one namespace to another.
        /// </summary>
        public List<NamespaceRemap> NamespaceRemaps { get; set; } = new List<NamespaceRemap>();

        /// <summary>
        /// Gets or sets mappings describing the translation from an input DTMI to an output DTMI.
        /// Note that only mappings where the input DTMI does not exactly match the output DTMI should be defined here.
        /// If the two DTMIs match exactly, they do not need to be added here.
        /// </summary>
        public List<DtmiRemap> InterfaceRemaps { get; set; } = new List<DtmiRemap>();

        /// <summary>
        /// Gets or sets mappings describing the translation from an input relationship type to an output relationship type.
        /// </summary>
        public List<RelationshipRemap> RelationshipRemaps { get; set; } = new List<RelationshipRemap>();

        /// <summary>
        /// Gets or sets property projections.
        /// In some cases, a property of the input model needs to be put into a different field or collection in the target model.
        /// Property projections allow declaration can be made to map the input field to the appropriate output field.
        /// </summary>
        public List<PropertyProjection> PropertyProjections { get; set; } = new List<PropertyProjection>();

        /// <summary>
        /// Gets or sets fill properties.
        /// In some cases, the contents of one input property may need to be copied to multiple other fields in the target ontology. For instance, if
        /// the target ontology requires that the name field always be populated, but the source name field may be null and the description field be more reliable,
        /// a chain of fields can be set here so that there is a priority list of fields that will backfill the name field if the input name field is null.
        /// </summary>
        public List<FillProperty> FillProperties { get; set; } = new List<FillProperty>();

        /// <summary>
        /// Gets or sets object transformations.
        /// In some cases, the contents of one input property may be an object which needs to be transformed to a field in the target ontology. For instance, if
        /// the target ontology requires that the unit field be a string, but the source property is an object, a transformation can be defined here to extract the unit field.
        /// </summary>
        public List<ObjectTransformation> ObjectTransformations { get; set; } = new List<ObjectTransformation>();
    }

    /// <summary>
    /// Metadata describing the mappings held in this file.
    /// </summary>
    public class MappingHeader
    {
        /// <summary>
        /// Gets or sets a list of the input ontologies used in this mapping.
        /// </summary>
        public List<Ontology> InputOntologies { get; set; } = new List<Ontology>();

        /// <summary>
        /// Gets or sets a list of the output ontologies used in this mapping.
        /// </summary>
        public List<Ontology> OutputOntologies { get; set; } = new List<Ontology>();
    }

    /// <summary>
    /// Defines how to map one input property into a target property if the names don't match.
    /// </summary>
    public class PropertyProjection
    {
        /// <summary>
        /// Gets or sets a regex describing the output DTMI's targeted by this projection. ".*" for all.
        /// </summary>
        public string OutputDtmiFilter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the output property in the target ontology.
        /// </summary>
        public string OutputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of input properties in the source ontology that map to the target.
        /// If target is a collection, this can also be a collection.
        /// </summary>
        public List<string> InputPropertyNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the output property is a collection.
        /// If it is, and if the input is a value type, set this to true to enable insert
        /// of the input value into the collection instead of straight assignment to the
        /// output property.
        /// </summary>
        public bool IsOutputPropertyCollection { get; set; } = false;

        /// <summary>
        /// Gets or sets projection priority.
        /// If there are multiple projections for a single output property based on different DtmiFilters,
        /// priority is taken into account in ascending order.
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Maps a priority list of properties on the input twin to a property on the output twin to ensure
    /// required properties have values.
    /// </summary>
    public class FillProperty
    {
        /// <summary>
        /// Gets or sets a regex describing the output DTMI's targeted by this fill. ".*" for all.
        /// </summary>
        public string OutputDtmiFilter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the output property in the target ontology.
        /// </summary>
        public string OutputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a space delimited, ordered, list of input properties to be assigned to the output property (loop until one is not empty).
        /// </summary>
        public IEnumerable<string> InputPropertyNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating for priority. If there are multiple projections for a single output property based on different DtmiFilters, priority is taken into account in ascending order.
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Maps an input interface from the source ontology to an output interface in the target ontology.
    /// </summary>
    public class DtmiRemap
    {
        /// <summary>
        /// Gets or sets the input DTMI as a string.
        /// </summary>
        public string InputDtmi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output DTMI as a string.
        /// </summary>
        public string OutputDtmi { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether or not this mapping is currently not ready for use (i.e. invalid), set this so that
        /// the mapping is declared in the file, but won't be used by the mapper.
        /// This allows reviewers to verify broken mappings in the future.
        /// </summary>
        public bool IsIgnored { get; set; } = false;
    }

    /// <summary>
    /// Maps an input relationship type from the source ontology to an output relationship type in the target ontology.
    /// </summary>
    public class RelationshipRemap
    {
        /// <summary>
        /// Gets or sets the input relationship type as a string.
        /// </summary>
        public string InputRelationship { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output relationship type as a string.
        /// </summary>
        public string OutputRelationship { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether or not the output mapping is intended to be a reverse mapping of the input.
        /// i.e. If in the source topology the mapping is from A to B, in the target topology, the mapping will be B to A.
        /// </summary>
        public bool ReverseRelationshipDirection { get; set; } = false;
    }

    /// <summary>
    /// Maps an input namespace from the source ontology to an output namespace in the target ontology.
    /// </summary>
    public class NamespaceRemap
    {
        /// <summary>
        /// Gets or sets the input namespace as a regex string.
        /// </summary>
        public string InputNamespace { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the output namespace as a regex string.
        /// </summary>
        public string OutputNamespace { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines how to map one input property into a target property if the names don't match.
    /// </summary>
    public class ObjectTransformation
    {
        /// <summary>
        /// Gets or sets a regex describing the output DTMI's targeted by this projection. ".*" for all.
        /// </summary>
        public string OutputDtmiFilter { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the output property in the target ontology.
        /// </summary>
        public string OutputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input property in the source ontology that map to the target.
        /// </summary>
        public string InputProperty { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the input property name in the source ontology that map to the target.
        /// </summary>
        public string InputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets projection priority.
        /// If there are multiple projections for a single output property based on different DtmiFilters,
        /// priority is taken into account in ascending order.
        /// </summary>
        public int Priority { get; set; } = 0;
    }

    /// <summary>
    /// Metadata describing a DTDL ontology.
    /// </summary>
    public class Ontology
    {
        /// <summary>
        /// Gets or sets the name of the ontology.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the version of the ontology.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the DTDL version of the ontology.
        /// </summary>
        public string DtdlVersion { get; set; } = string.Empty;
    }
}
