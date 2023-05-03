// -----------------------------------------------------------------------
// <copyright file="IngestionProcessorBaseTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.DigitalTwins.Parser;
    using global::Azure.DigitalTwins.Core;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class IngestionProcessorBaseTests
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        private readonly Mock<ILogger<MockedIngestionProcessor<IngestionManagerOptions>>> mockLogger;
        private readonly Mock<IInputGraphManager> mockInputGraphManager;
        private readonly Mock<IOutputGraphManager> mockOutputGraphManager;
        private readonly OntologyMappingManager ontologyMappingManager;
        private readonly TelemetryClient telemetryClient;

        public IngestionProcessorBaseTests(ITestOutputHelper output)
        {
            this.output = output;
            mockLogger = new Mock<ILogger<MockedIngestionProcessor<IngestionManagerOptions>>>();
            mockInputGraphManager = new Mock<IInputGraphManager>();

            var spaceDtmi = "dtmi:org:w3id:rec:Space;1";
            var spaceWithBoxDtmi = "dtmi:org:w3id:rec:SpaceWithBox;1";

            mockInputGraphManager.Setup(m => m.TryGetDtmi("Space", out spaceDtmi)).Returns(true);
            mockInputGraphManager.Setup(m => m.TryGetDtmi("SpaceWithBox", out spaceWithBoxDtmi)).Returns(true);

            mockOutputGraphManager = new Mock<IOutputGraphManager>();

            var listOfDtdlFiles = new List<string>
                {
                    "IngestionManager.Test.TestData.Box.json",
                    "IngestionManager.Test.TestData.Space.json",
                    "IngestionManager.Test.TestData.SpaceWithBox.json",
                };

            mockOutputGraphManager.Setup(m => m.GetModelAsync(CancellationToken.None)).ReturnsAsync(LoadDtdl(listOfDtdlFiles));

            TelemetryConfiguration appInsightsConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new Mock<ITelemetryChannel>().Object,
            };

            telemetryClient = new TelemetryClient(appInsightsConfiguration);

            var mockOntologyLoader = new Mock<IOntologyMappingLoader>();
            mockOntologyLoader.Setup(m => m.LoadOntologyMapping()).Returns(GetTestMappings);

            ontologyMappingManager = new OntologyMappingManager(mockOntologyLoader.Object);
        }

        [Fact]
        public async Task IngestFromApi_NoData()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
        }

        [Fact]
        public void GetInputInterfaceDtmi_ReturnsNull_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var dtmi = mockedIngestionProcessor.TestInputInterfaceDtmi("invalidType");
            Assert.Null(dtmi);
        }

        [Fact]
        public void GetInputInterfaceDtmi_ReturnsDtmi_WhenFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var dtmi = mockedIngestionProcessor.TestInputInterfaceDtmi("Space");
            Assert.NotNull(dtmi);
        }

        [Fact]
        public void GetOutputRelationshipType_ReturnsString_WhenFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedRelationshipType = "isA";
            var outputRelationshipType = mockedIngestionProcessor.TestGetOutputRelationshipType("hasA");

            Assert.Equal(expectedRelationshipType, outputRelationshipType);
        }

        [Fact]
        public void GetOutputRelationshipType_ReturnsInputValue_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var inputRelationshipType = "wasA";
            var outputRelationshipType = mockedIngestionProcessor.TestGetOutputRelationshipType(inputRelationshipType);
            Assert.Equal(inputRelationshipType, outputRelationshipType);
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsValue_WhenFoundInTargetOntology()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var inputDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.True(result);
            Assert.NotNull(outputDtmi);
            Assert.Equal(inputDtmi.ToString(), outputDtmi?.ToString());
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsValue_WhenFoundInInterfaceRemap()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var inputDtmi = new Dtmi("dtmi:twin:main:CleaningRoom;1");
            var expectedOutputInterface = new Dtmi("dtmi:org:w3id:rec:Space;1");
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.True(result);
            Assert.NotNull(outputDtmi);
            Assert.Equal(expectedOutputInterface.ToString(), outputDtmi?.ToString());
        }

        [Fact]
        public async Task TryGetOutputInterfaceDtmi_ReturnsNull_WhenNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var inputDtmi = new Dtmi("dtmi:twin:main:SpaceShip;1");
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            var result = mockedIngestionProcessor.TestTryGetOutputInterfaceDtmi(inputDtmi, out var outputDtmi);
            Assert.False(result);
            Assert.Null(outputDtmi);
        }

        [Fact]
        public async Task GetTwin_ReturnsNull_WhenInputInterfaceNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = string.Empty;
            string interfaceType = "SpaceShip";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTwin_ReturnsNull_WhenOutputInterfaceNotFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = string.Empty;
            string interfaceType = "Box";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTwin_ReturnsDtmi_WhenOutputMappingFound()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElement();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Single(twins.First().Value.Contents);
            Assert.Equal("AV 31", twins.First().Value.Contents["name"].ToString());
        }

        [Fact]
        public async Task GetTwin_FillsProperty_WhenFillPropertySpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithEmptyName();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Single(twins.First().Value.Contents);
            Assert.Equal("test", twins.First().Value.Contents["name"].ToString());
        }

        [Fact]
        public async Task GetTwin_FillsComponent_WhenComponentNotSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedDtmi = "dtmi:org:w3id:rec:SpaceWithBox;1";
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithEmptyName();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "SpaceWithBox";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(2, twins.First().Value.Contents.Count);
            Assert.Equal("test", twins.First().Value.Contents["name"].ToString());
            Assert.Equal("{ \"$metadata\": {} }", twins.First().Value.Contents["box"].ToString());
        }

        [Fact]
        public async Task GetTwin_ProjectsProperty_WhenPropertyProjectionSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithMappingKey();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(2, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as IDictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
        }

        [Fact]
        public async Task GetTwin_ProjectsProperty_WhenPropertyProjectionForMultiplePropertiesSpecified()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            var expectedDtmi = "dtmi:org:w3id:rec:Space;1";
            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
            IDictionary<string, BasicDigitalTwin> twins = new Dictionary<string, BasicDigitalTwin>();
            JsonElement jsonElement = GetJsonElementWithMultipleKeys();
            string basicDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            string interfaceType = "Space";

            var result = mockedIngestionProcessor.TestGetTwin(twins, jsonElement, basicDtId, interfaceType);
            Assert.NotNull(result);
            Assert.Equal(expectedDtmi, result?.ToString());
            Assert.Single(twins);
            Assert.Equal(basicDtId, twins.First().Key);
            Assert.Equal(basicDtId, twins.First().Value.Id);
            Assert.Equal(expectedDtmi, twins.First().Value.Metadata.ModelId);
            Assert.Equal(2, twins.First().Value.Contents.Count);

            var contents = twins.First().Value.Contents as IDictionary<string, object>;
            var externalIds = contents["externalIds"] as IDictionary<string, string>;
            Assert.NotNull(externalIds);
            Assert.Equal("12345", externalIds?["mappingKey"]);
            Assert.Equal("678", externalIds?["deviceId"]);
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenValidRelationship()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            IDictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "isLocationOf";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";

            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(inputRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.SourceId);
            Assert.Equal(targetDtId, outputRelationship.Value.TargetId);
            Assert.Equal($"{sourceDtId}-{targetDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenRemappedRelationship()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            IDictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "hasA";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";

            var expectedRelationshipType = "isA";

            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(expectedRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.SourceId);
            Assert.Equal(targetDtId, outputRelationship.Value.TargetId);
            Assert.Equal($"{sourceDtId}-{targetDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
        }

        [Fact]
        public async Task GetRelationship_GetsRelationship_WhenRemappedRelationshipReversed()
        {
            var mockedIngestionProcessor = new MockedIngestionProcessor<IngestionManagerOptions>(mockLogger.Object,
                                                                        telemetryClient,
                                                                        mockInputGraphManager.Object,
                                                                        ontologyMappingManager,
                                                                        mockOutputGraphManager.Object);

            IDictionary<string, BasicRelationship> relationships = new Dictionary<string, BasicRelationship>();
            string sourceDtId = "CLSKkDFMgbojZZ54MorD6B11P";
            var inputSourceDtmi = new Dtmi("dtmi:org:w3id:rec:Space;1");
            var inputRelationshipType = "hasPoint";
            string targetDtId = "CLSKkDFMgbojZZ54MorD6B11R";
            string targetInterfaceType = "Space";

            var expectedRelationshipType = "isPointOf";

            await mockedIngestionProcessor.IngestFromApiAsync(CancellationToken.None);

            mockedIngestionProcessor.TestGetRelationship(relationships, sourceDtId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType);

            Assert.Single(relationships);
            var outputRelationship = relationships.First();

            Assert.NotNull(outputRelationship.Value);
            Assert.Equal(expectedRelationshipType, outputRelationship.Value.Name);
            Assert.Equal(sourceDtId, outputRelationship.Value.TargetId);
            Assert.Equal(targetDtId, outputRelationship.Value.SourceId);
            Assert.Equal($"{targetDtId}-{sourceDtId}-{outputRelationship.Value.Name}", outputRelationship.Value.Id);
            Assert.NotNull(outputRelationship.Value.Id);
        }

        private static JsonElement GetJsonElement()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11R\", \"name\": \"AV 31\", \"exactType\": \"TemperatureAlarmSetpoint\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithEmptyName()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"name\": null, \"description\": \"test\", \"exactType\": \"TemperatureAlarmSetpoint\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithMappingKey()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"exactType\": \"TemperatureAlarmSetpoint\", \"mappingKey\": \"12345\" }");
            return doc.RootElement;
        }

        private static JsonElement GetJsonElementWithMultipleKeys()
        {
            var doc = JsonDocument.Parse("{ \"id\": \"CLSKkDFMgbojZZ54MorD6B11P\", \"exactType\": \"TemperatureAlarmSetpoint\", \"mappingKey\": \"12345\", \"deviceId\": \"678\" }");
            return doc.RootElement;
        }

        private OntologyMapping GetTestMappings()
        {
            var ontologyMapping = new OntologyMapping();

            ontologyMapping.Header.InputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "twin", Version = "1.0" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v2", Name = "org1", Version = "1.1" });
            ontologyMapping.Header.OutputOntologies.Add(new Ontology { DtdlVersion = "v3", Name = "org2", Version = "1.2" });
            ontologyMapping.InterfaceRemaps.Add(new DtmiRemap { InputDtmi = "dtmi:twin:main:CleaningRoom;1", OutputDtmi = "dtmi:org:w3id:rec:Space;1" });
            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "hasA", OutputRelationship = "isA" });
            ontologyMapping.RelationshipRemaps.Add(new RelationshipRemap { InputRelationship = "hasPoint", OutputRelationship = "isPointOf", ReverseRelationshipDirection = true });
            ontologyMapping.FillProperties.Add(new FillProperty { InputPropertyNames = new List<string>() { "name", "description" }, OutputDtmiFilter = ".*", OutputPropertyName = "name" });
            ontologyMapping.PropertyProjections.Add(new PropertyProjection { InputPropertyNames = new List<string> { "mappingKey", "deviceId" }, OutputDtmiFilter = ".*", IsOutputPropertyCollection = true, OutputPropertyName = "externalIds" });
            return ontologyMapping;
        }

        private static List<string> LoadDtdl(List<string> fileNames)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var jsonTexts = new List<string>();

            foreach (var fileName in fileNames)
            {
                var resourceName = resources.Single(str => str.EndsWith(fileName));

                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string? result = reader.ReadToEnd();

                            jsonTexts.Add(result);
                        }
                    }
                }
            }

            return jsonTexts;
        }
    }

    public class MockedIngestionProcessor<TOptions> : IngestionProcessorBase<TOptions>
        where TOptions : IngestionManagerOptions
    {
        public MockedIngestionProcessor(ILogger<MockedIngestionProcessor<TOptions>> logger,
                                        TelemetryClient telemetryClient,
                                        IInputGraphManager inputGraphManager,
                                        IOntologyMappingManager ontologyMappingManager,
                                        IOutputGraphManager outputGraphManager)
            : base(logger, inputGraphManager, ontologyMappingManager, outputGraphManager, telemetryClient)
        {
        }

        public Dtmi? TestInputInterfaceDtmi(string interfaceType)
        {
            return GetInputInterfaceDtmi(interfaceType);
        }

        public string TestGetOutputRelationshipType(string inputRelationshipType)
        {
            return GetOutputRelationshipType(inputRelationshipType).Item1;
        }

        public bool TestTryGetOutputInterfaceDtmi(Dtmi inputDtmi, out Dtmi? outputDtmi)
        {
            return TryGetOutputInterfaceDtmi(inputDtmi, out outputDtmi);
        }

        public Dtmi? TestGetTwin(IDictionary<string, BasicDigitalTwin> twins,
                      JsonElement targetElement,
                      string basicDtId,
                      string interfaceType)
        {
            return AddTwin(twins, targetElement, basicDtId, interfaceType);
        }

        public void TestGetRelationship(IDictionary<string, BasicRelationship> relationships,
                              string? sourceElementId,
                              Dtmi? inputSourceDtmi,
                              string? inputRelationshipType,
                              string targetDtId,
                              string targetInterfaceType)
        {
            AddRelationship(relationships, sourceElementId, inputSourceDtmi, inputRelationshipType, targetDtId, targetInterfaceType);
        }

        protected override Task ProcessSites(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}