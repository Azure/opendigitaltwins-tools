// -----------------------------------------------------------------------
// <copyright file="OntologyMapping.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the structure of the files stored in the Mappings folder for deserialization
    /// </summary>
    public class OntologyMapping
    {
        /// <summary>
        /// Information regarding the contents of the file for reference purposes
        /// </summary>
        public MappingHeader Header { get; set; } = new MappingHeader();

        /// <summary>
        /// Mappings describing the translation from an input DTMI to an output DTMI. Note that only mappings where the input DTMI does not exactly match the output DTMI should be defined here.
        /// If the two DTMIs match exactly, they do not need to be added here
        /// </summary>
        public List<DtmiRemap> InterfaceRemaps { get; set; } = new List<DtmiRemap>();

        /// <summary>
        /// Mappings describing the translation from an input relationship type to an output relationship type
        /// </summary>
        public List<RelationshipRemap> RelationshipRemaps { get; set; } = new List<RelationshipRemap>();

        /// <summary>
        /// In some cases, a property of the input model needs to be put into a different field or collection in the target model. A declaration can be made to map the input field to the appropriate output field
        /// </summary>
        public List<PropertyProjection> PropertyProjections { get; set; } = new List<PropertyProjection>();

        /// <summary>
        /// In some cases, the contents of one input property may need to be copied to multiple other fields in the target ontology. For instance, if 
        /// the target ontology requires that the name field always be populated, but the source name field may be null and the description field be more reliable,
        /// a chain of fields can be set here so that there is a priority list of fields that will backfill the name field if the input name field is null.
        /// </summary>
        public List<FillProperty> FillProperties { get; set; } = new List<FillProperty>();
    }

    /// <summary>
    /// Describes the contents of the mapping file
    /// </summary>
    public class MappingHeader
    {
        /// <summary>
        /// A list of the input ontologies used in this mapping
        /// </summary>
        public List<Ontology> InputOntologies { get; set; } = new List<Ontology>();

        /// <summary>
        /// A list of the output ontologies used in this mapping
        /// </summary>
        public List<Ontology> OutputOntologies { get; set; } = new List<Ontology>();
    }

    /// <summary>
    /// Defines how to map one input property into a target property if the names don't match
    /// </summary>
    public class PropertyProjection
    {
        /// <summary>
        /// A regex describing the output DTMI's targeted by this projection. * for all
        /// </summary>
        public string OutputDtmiFilter { get; set; } = string.Empty;

        /// <summary>
        /// The name of the output property in the target ontology
        /// </summary>
        public string OutputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// The name of the input property in the source ontology
        /// </summary>
        public string InputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// If the output is a collection, and the input is a value type, set this to 
        /// true to enable insert of the input value into the collection instead of straight
        /// assignment to the output property
        /// </summary>
        public bool IsOutputPropertyCollection { get; set; } = false;
    }

    /// <summary>
    /// Maps a priority list of properties on the input twin to a property on the output twin to ensure
    /// required properties have values
    /// </summary>
    public class FillProperty
    {
        /// <summary>
        /// A regex describing the output DTMI's targeted by this fill. * for all
        /// </summary>
        public string OutputDtmiFilter { get; set; } = string.Empty;

        /// <summary>
        /// The name of the output property in the target ontology
        /// </summary>
        public string OutputPropertyName { get; set; } = string.Empty;

        /// <summary>
        /// A space delimited, ordered, list of input properties to be assigned to the output property (loop until one is not empty)
        /// </summary>
        public IEnumerable<string> InputPropertyNames { get; set; } = new List<string>();
    }

    /// <summary>
    /// Maps an input input from the source ontology to an output interface in the target ontology
    /// </summary>
    public class DtmiRemap
    {
        /// <summary>
        /// The input DTMI as a string
        /// </summary>
        public string InputDtmi { get; set; } = string.Empty;

        /// <summary>
        /// The output DTMI as a string
        /// </summary>
        public string OutputDtmi { get; set; } = string.Empty;

        /// <summary>
        /// If this mapping is currently not ready for use (i.e. invalid), set this so that 
        /// the mapping is declared in the file, but won't be used by the mapper
        /// This allows reviewers to verify broken mappings in the future
        /// </summary>
        public bool IsIgnored { get; set; } = false;
    }

    /// <summary>
    /// Maps an input relationship type from the source ontology to an output relationship type in the target ontology
    /// </summary>
    public class RelationshipRemap
    {
        /// <summary>
        /// The input relationship type as a string
        /// </summary>
        public string InputRelationship { get; set; } = string.Empty;

        /// <summary>
        /// The output relationship type as a string
        /// </summary>
        public string OutputRelationship { get; set; } = string.Empty;
    }

    public class Ontology
    {
        /// <summary>
        /// The name of the ontology
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The version of the ontology
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// The DTDL version of the ontology
        /// </summary>
        public string DtdlVersion { get; set; } = string.Empty;
    }
}
