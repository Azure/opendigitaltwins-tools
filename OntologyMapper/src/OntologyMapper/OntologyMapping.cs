// -----------------------------------------------------------------------
// <copyright file="OntologyMapping.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Aspen.OntologyMapper
{
    using System.Collections.Generic;

    public class OntologyMapping
    {
        public MappingHeader Header { get; set; } = new MappingHeader();

        public List<DtmiRemap> InterfaceRemaps { get; set; } = new List<DtmiRemap>();

        public List<RelationshipRemap> RelationshipRemaps { get; set; } = new List<RelationshipRemap>();

        public List<PropertyProjection> PropertyProjections { get; set; } = new List<PropertyProjection>();

        public List<FillProperty> FillProperties { get; set; } = new List<FillProperty>();
    }

    public class MappingHeader
    {
        public List<Ontology> InputOntologies { get; set; } = new List<Ontology>();

        public List<Ontology> OutputOntologies { get; set; } = new List<Ontology>();
    }

    public class PropertyProjection
    {
        public string OutputDtmiFilter { get; set; } = string.Empty;

        public string OutputPropertyName { get; set; } = string.Empty;

        public string InputPropertyName { get; set; } = string.Empty;

        public bool IsOutputPropertyCollection { get; set; } = false;
    }

    public class FillProperty
    {
        public string OutputDtmiFilter { get; set; } = string.Empty;

        public string OutputPropertyName { get; set; } = string.Empty;

        public string InputPropertyNames { get; set; } = string.Empty;
    }

    public class DtmiRemap
    {
        public string InputDtmi { get; set; } = string.Empty;

        public string OutputDtmi { get; set; } = string.Empty;

        public bool IsIgnored { get; set; } = false;
    }

    public class RelationshipRemap
    {
        public string InputRelationship { get; set; } = string.Empty;

        public string OutputRelationship { get; set; } = string.Empty;
    }

    public class Ontology
    {
        public string Name { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string DtdlVersion { get; set; } = string.Empty;
    }
}
