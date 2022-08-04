// -----------------------------------------------------------------------
// <copyright file="OntologyMappingTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace OntologyMapper.Test
{
    using Microsoft.Azure.Aspen.OntologyMapper;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    public class OntologyMappingTests
    {
        private readonly ITestOutputHelper output;

        public OntologyMappingTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void CreateSimpleOntologyMapping()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v0", Name = "mapped", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "brick", Version = "1.3" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "rec", Version = "4.0" });

            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = "*", InputPropertyName = "mappingKey", OutputPropertyName = "externalIds", IsOutputPropertyCollection = true });

            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = "*", OutputPropertyName = "name", InputPropertyNames = "name description" });

            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "inputDtmi1", OutputDtmi = "outputDtmi1" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "inputDtmi2", OutputDtmi = "outputDtmi2" });

            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "isA", OutputRelationship = "wasA" });

            var serialized = JsonConvert.SerializeObject(ontologyMapping, Formatting.Indented);
            output.WriteLine(serialized);
        }
    }
}