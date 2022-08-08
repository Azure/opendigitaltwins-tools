// -----------------------------------------------------------------------
// <copyright file="OntologyMappingTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper.Test
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartFacilities.OntologyMapper;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class OntologyMappingManagerTests
    {
        private readonly ITestOutputHelper output;

        public OntologyMappingManagerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TryGetInterfaceRemapDtmi_ReturnsTrue_When_Found()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var inputDtmi = new Dtmi("dtmi:twin:main:CleaningRoom;1");
            var outputDtmi = new Dtmi("dtmi:org:w3id:rec:CleanRoom;1");

            var result = ontologyMappingManager.TryGetInterfaceRemapDtmi(inputDtmi, out var dtmiRemap);

            Assert.True(result);
            Assert.NotNull(dtmiRemap);
            Assert.Equal(inputDtmi.ToString(), dtmiRemap.InputDtmi);
            Assert.Equal(outputDtmi.ToString(), dtmiRemap.OutputDtmi);
            Assert.False(dtmiRemap.IsIgnored);
        }

        [Fact]
        public void TryGetInterfaceRemapDtmi_ReturnsFalse_When_IsIgnoredTrue()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var inputDtmi = new Dtmi("dtmi:twin:main:RandomEquipment;1");

            var result = ontologyMappingManager.TryGetInterfaceRemapDtmi(inputDtmi, out var dtmiRemap);

            Assert.False(result);
            Assert.Null(dtmiRemap);
        }

        [Fact]
        public void TryGetInterfaceRemapDtmi_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var inputDtmi = new Dtmi("dtmi:twin:main:FishCleaningRoom;1");

            var result = ontologyMappingManager.TryGetInterfaceRemapDtmi(inputDtmi, out var dtmiRemap);

            Assert.False(result);
            Assert.Null(dtmiRemap);
        }

        [Fact]
        public void TryGetRelationshipRemapDtmi_ReturnsTrue_When_Found()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var inputRelationship = "isA";
            var expectedOutputRelationship = "wasA";

            var result = ontologyMappingManager.TryGetRelationshipRemap(inputRelationship, out var outputRelationship);

            Assert.True(result);
            Assert.NotNull(outputRelationship);
            Assert.Equal(expectedOutputRelationship, outputRelationship.OutputRelationship);
        }

        [Fact]
        public void TryGetRelationshipRemapDtmi_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var inputRelationship = "isnotA";

            var result = ontologyMappingManager.TryGetRelationshipRemap(inputRelationship, out var outputRelationship);

            Assert.False(result);
            Assert.Null(outputRelationship);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsTrue_When_Found()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmiFilter = "*";
            var outputPropertyName = "externalIds";
            var inputPropertyName = "deviceKey";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmiFilter, outputPropertyName, out var propertyProjection);

            Assert.True(result);
            Assert.NotNull(propertyProjection);
            Assert.Equal(outputDtmiFilter, propertyProjection.OutputDtmiFilter);
            Assert.Equal(outputPropertyName, propertyProjection.OutputPropertyName);
            Assert.Equal(inputPropertyName, propertyProjection.InputPropertyName);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmiFilter = "*";
            var outputPropertyName = "external";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmiFilter, outputPropertyName, out var propertyProjection);

            Assert.False(result);
            Assert.Null(propertyProjection);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsTrue_When_Found()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmiFilter = "*";
            var outputPropertyName = "name";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmiFilter, outputPropertyName, out var propertyFill);

            Assert.True(result);
            Assert.NotNull(propertyFill);
            Assert.Equal(2, propertyFill.InputPropertyNames.Count());
            Assert.Equal("name", propertyFill.InputPropertyNames.ToList()[0]);
            Assert.Equal("description", propertyFill.InputPropertyNames.ToList()[1]);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmiFilter = "*";
            var outputPropertyName = "channel";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmiFilter, outputPropertyName, out var propertyFill);

            Assert.False(result);
            Assert.Null(propertyFill);
        }

        [Theory]
        [InlineData("Mappings.Mapped.Json.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json")]
        [InlineData("Mappings.Mapped.Json.v0.BrickRec.mapped_json_v0_dtdlv3_Brick_1_3-REC_4_0.json")]
        public void ValidateEmbeddedResourceDtmisAreValidFormat(string resourcePath)
        {
            var mockLogger = new Mock<ILogger>();
            var resourceLoader = new EmbeddedResourceOntologyMappingLoader(mockLogger.Object, resourcePath);
            var ontologyMappingManager = new OntologyMappingManager(resourceLoader);

            var exceptions = new List<string>();
            foreach (var mapping in ontologyMappingManager.OntologyMapping.InterfaceRemaps)
            {
                try
                {
                    var inputDtmi = new Dtmi(mapping.InputDtmi);
                }
                catch (ParsingException)
                {
                    exceptions.Add($"Invalid input DTMI: {mapping.InputDtmi}");
                }

                try
                {
                    var outputDtmi = new Dtmi(mapping.OutputDtmi);
                }
                catch (ParsingException)
                {
                    exceptions.Add($"Invalid output DTMI: {mapping.OutputDtmi}");
                }
            }

            Assert.Empty(exceptions);
        }

        private OntologyMapping GetOntologyMapping()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });

            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = "*", InputPropertyName = "deviceKey", OutputPropertyName = "externalIds", IsOutputPropertyCollection = true });

            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = "*", OutputPropertyName = "name", InputPropertyNames = new string[] { "name", "description" } });

            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:CleanRoom;1" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:RandomRoom;1", OutputDtmi = "dtmi:org:org1:schema:test:Office;1", IsIgnored = true });

            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "isA", OutputRelationship = "wasA" });

            return ontologyMapping;
        }

        //private IReadOnlyDictionary<Dtmi, DTEntityInfo> GetTargetObjectModel()
        //{
        //    var targetObjectModel = new Dictionary<Dtmi, DTEntityInfo>();

        //    targetObjectModel.Add(new Dtmi("dtmi:mapped:core:AblutionsRoom;1"), new DTEntityInfo());

        //    return targetObjectModel;
        //}
    }
}