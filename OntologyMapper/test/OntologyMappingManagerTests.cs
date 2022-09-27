// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace OntologyMapper.Test
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;
    using Moq;
    using System.Reflection;
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
        public void TryGetPropertyProjection_ReturnsTrue_WhenFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "externalIds";
            var inputPropertyName = "deviceKey";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjection);

            Assert.True(result);
            Assert.NotNull(propertyProjection);
            Assert.Equal(outputPropertyName, propertyProjection.OutputPropertyName);
            Assert.Equal(inputPropertyName, propertyProjection.InputPropertyNames[0]);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsTrue_WhenRegexMatchFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Motor;1";
            var outputPropertyName = "externalMotors";
            var inputPropertyName = "deviceMotor";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjection);

            Assert.True(result);
            Assert.NotNull(propertyProjection);
            Assert.Equal(outputPropertyName, propertyProjection.OutputPropertyName);
            Assert.Equal(inputPropertyName, propertyProjection.InputPropertyNames[0]);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsTrue_WhenRegexMatchFoundSecondPriority()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Drive;1";
            var outputPropertyName = "externalMotors";
            var inputPropertyName = "deviceStand";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjection);

            Assert.True(result);
            Assert.NotNull(propertyProjection);
            Assert.Equal(outputPropertyName, propertyProjection.OutputPropertyName);
            Assert.Equal(inputPropertyName, propertyProjection.InputPropertyNames[0]);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsFalse_WhenRegexMatchNotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Sport;1";
            var outputPropertyName = "externalDrive";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjection);

            Assert.False(result);
            Assert.Null(propertyProjection);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsTrue_WhenFoundMultiple()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMappingWithMultipleProjections);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "externalIds";
            var inputPropertyName1 = "deviceKey";
            var inputPropertyName2 = "deviceId";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjections);

            Assert.True(result);
            Assert.NotNull(propertyProjections);
            Assert.Equal(outputPropertyName, propertyProjections.OutputPropertyName);
            Assert.Equal(inputPropertyName1, propertyProjections.InputPropertyNames[0]);
            Assert.Equal(inputPropertyName2, propertyProjections.InputPropertyNames[1]);
        }

        [Fact]
        public void TryGetPropertyProjection_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "external";

            var result = ontologyMappingManager.TryGetPropertyProjection(outputDtmi, outputPropertyName, out var propertyProjection);

            Assert.False(result);
            Assert.Null(propertyProjection);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsTrue_When_Found()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "name";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmi, outputPropertyName, out var propertyFill);

            Assert.True(result);
            Assert.NotNull(propertyFill);
            Assert.Equal(2, propertyFill.InputPropertyNames.Count());
            Assert.Equal("name", propertyFill.InputPropertyNames.ToList()[0]);
            Assert.Equal("description", propertyFill.InputPropertyNames.ToList()[1]);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsTrue_When_FoundByRegex()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Motor;1";
            var outputPropertyName = "fan";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmi, outputPropertyName, out var propertyFill);

            Assert.True(result);
            Assert.NotNull(propertyFill);
            Assert.Equal(2, propertyFill.InputPropertyNames.Count());
            Assert.Equal("name", propertyFill.InputPropertyNames.ToList()[0]);
            Assert.Equal("description", propertyFill.InputPropertyNames.ToList()[1]);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsTrue_When_FoundByRegexSecondPriority()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "fan";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmi, outputPropertyName, out var propertyFill);

            Assert.True(result);
            Assert.NotNull(propertyFill);
            Assert.Equal(2, propertyFill.InputPropertyNames.Count());
            Assert.Equal("name", propertyFill.InputPropertyNames.ToList()[0]);
            Assert.Equal("size", propertyFill.InputPropertyNames.ToList()[1]);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsFalse_When_NotFoundByRegex()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Motor;1";
            var outputPropertyName = "switch";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmi, outputPropertyName, out var propertyFill);

            Assert.False(result);
            Assert.Null(propertyFill);
        }

        [Fact]
        public void TryGetFillProperty_ReturnsFalse_When_NotFound()
        {
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetOntologyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var outputDtmi = "dtmi:org:w3id:rec:Space;1";
            var outputPropertyName = "channel";

            var result = ontologyMappingManager.TryGetFillProperty(outputDtmi, outputPropertyName, out var propertyFill);

            Assert.False(result);
            Assert.Null(propertyFill);
        }

        [Fact]
        public void ValidateTargetOntologyMapping_ReturnsTrue_ForValidOntologyMapping()
        {
            var targetObjectModel = GetTargetObjectModel();
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetSpaceOnlyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var result = ontologyMappingManager.ValidateTargetOntologyMapping(targetObjectModel, out var invalidTargets);

            Assert.True(result);
            Assert.False(invalidTargets.Any());
        }

        [Fact]
        public void ValidateTargetOntologyMapping_ReturnsFalse_ForInvalidOntologyMapping()
        {
            var targetObjectModel = GetTargetObjectModel();
            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetBuildingOnlyMapping);

            var ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);

            var result = ontologyMappingManager.ValidateTargetOntologyMapping(targetObjectModel, out var invalidTargets);

            Assert.False(result);
            Assert.Single(invalidTargets);
            Assert.Equal("dtmi:org:w3id:rec:Building;1", invalidTargets[0]);
        }

        private OntologyMapping GetOntologyMapping()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });

            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = "*", InputPropertyNames = new List<string> { "deviceKey" }, OutputPropertyName = "externalIds", IsOutputPropertyCollection = true });
            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = @"\w*Motor\w*", InputPropertyNames = new List<string> { "deviceMotor" }, OutputPropertyName = "externalMotors", IsOutputPropertyCollection = true, Priority = 0 });
            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = @"*", InputPropertyNames = new List<string> { "deviceStand" }, OutputPropertyName = "externalMotors", IsOutputPropertyCollection = true, Priority = 1 });
            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = @"\w*Drive\w*", InputPropertyNames = new List<string> { "deviceDrive" }, OutputPropertyName = "externalDrive", IsOutputPropertyCollection = true, Priority = 0 });

            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = "*", OutputPropertyName = "name", InputPropertyNames = new string[] { "name", "description" } });
            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = @"\w*Motor\w*", OutputPropertyName = "fan", InputPropertyNames = new string[] { "name", "description" }, Priority = 1 });
            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = @"*", OutputPropertyName = "fan", InputPropertyNames = new string[] { "name", "size" }, Priority = 2 });

            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:CleanRoom;1" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:RandomRoom;1", OutputDtmi = "dtmi:org:org1:schema:test:Office;1", IsIgnored = true });

            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "isA", OutputRelationship = "wasA" });

            return ontologyMapping;
        }

        private OntologyMapping GetOntologyMappingWithMultipleProjections()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });

            ontologyMapping.PropertyProjections.Add(new PropertyProjection { OutputDtmiFilter = "*", InputPropertyNames = new List<string> { "deviceKey", "deviceId" }, OutputPropertyName = "externalIds", IsOutputPropertyCollection = true });

            ontologyMapping.FillProperties.Add(new FillProperty { OutputDtmiFilter = "*", OutputPropertyName = "name", InputPropertyNames = new string[] { "name", "description" } });

            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:CleanRoom;1" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:RandomRoom;1", OutputDtmi = "dtmi:org:org1:schema:test:Office;1", IsIgnored = true });

            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "isA", OutputRelationship = "wasA" });

            return ontologyMapping;
        }

        private OntologyMapping GetSpaceOnlyMapping()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:Space;1" });
            return ontologyMapping;
        }

        private OntologyMapping GetBuildingOnlyMapping()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:Tower;1", OutputDtmi = "dtmi:org:w3id:rec:Building;1" });
            return ontologyMapping;
        }

        private static IReadOnlyDictionary<Dtmi, DTEntityInfo> GetTargetObjectModel()
        {
            var objectModelParser = new ModelParser();
            var jsonTexts = LoadDtdl("Space.json");
            return objectModelParser.Parse(jsonTexts);
        }

        private static List<string> LoadDtdl(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.Single(str => str.EndsWith(fileName));
            var jsonTexts = new List<string>();

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using StreamReader reader = new(stream);
                    string? result = reader.ReadToEnd();

                    jsonTexts.Add(result);
                }
            }

            return jsonTexts;
        }
    }
}