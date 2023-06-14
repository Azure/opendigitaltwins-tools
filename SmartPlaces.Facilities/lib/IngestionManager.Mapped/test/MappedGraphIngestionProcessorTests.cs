// -----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessorTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped.Test
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class MappedGraphIngestionProcessorTests
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        private const string organizationQuery = "{sites{description,exactType,id,name}}";
        private const string siteQuery = "{ sites(filter: { id: { eq: \"SITE44nUdjjbqSX1sEXuwEWucr\"} }) { description,exactType,id,name,dateCreated,dateUpdated,type,buildings{ address{countryName,dateCreated,dateUpdated,id,locality,postalCode,region,streetAddress},description,exactType,id,identities { ... on ExternalIdentity { dateCreated, dateUpdated, value } },name,type,floors{ dateCreated,dateUpdated,description,exactType,id,level,name,type} } } }";
        private const string buildingThingsQuery = "{ buildings(filter: { id: { eq: \"BLDG5o26DguWKu5T9nRvSYn5Em\"} }) { things{ dateCreated,dateUpdated,description,exactType,firmwareVersion,id,mappingKey,model { id,description,manufacturer { id,name,description,logoUrl }, manufacturerId,name,imageUrl,seeAlsoUrls },name,hasLocation{ exactType,id,name },isFedBy{ id,name,exactType }} } }";
        
        private JsonDocument? siteJsonDocument;
        private JsonDocument? organizationJsonDocument;
        private JsonDocument? buildingThingsJsonDocument;

        public MappedGraphIngestionProcessorTests(ITestOutputHelper output)
        {
            this.output = output;

            var organizationJsonFile = System.IO.File.ReadAllText("data/organization.json");
            var organizationReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(organizationJsonFile));
            _ = JsonDocument.TryParseValue(ref organizationReader, out organizationJsonDocument);

            var siteJsonFile = System.IO.File.ReadAllText("data/site.json");
            var siteReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(siteJsonFile));
            _ = JsonDocument.TryParseValue(ref siteReader, out siteJsonDocument);

            var buildingThingsJsonFile = System.IO.File.ReadAllText("data/buildingThings.json");
            var buildingThingsReader = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(buildingThingsJsonFile));
            _ = JsonDocument.TryParseValue(ref buildingThingsReader, out buildingThingsJsonDocument);
        }

        [Fact]
        public async Task IngestFromApi_NoData()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();

            var mockInputGraphManager = new Mock<IInputGraphManager>();

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();

            TelemetryConfiguration appInsightsConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new Mock<ITelemetryChannel>().Object,
            };

            var telemetryClient = new TelemetryClient(appInsightsConfiguration);

            var mockOntologyMappingManager = new Mock<IOntologyMappingManager>();

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager.Object, mockOntologyMappingManager.Object, mockOutputGraphManager.Object, graphNamingManager, telemetryClient);

            await graphIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
        }

        [Fact]
        public async Task IngestFromApi_OneBuilding()
        {
            var mockLogger = new Mock<ILogger<MappedGraphIngestionProcessor<IngestionManagerOptions>>>();
            
            var mockInputGraphManager = new Mock<IInputGraphManager>();
            
            mockInputGraphManager.Setup(x => x.GetOrganizationQuery()).Returns(organizationQuery);
            mockInputGraphManager.Setup(x => x.GetBuildingsForSiteQuery(It.IsAny<string>())).Returns(siteQuery);
            mockInputGraphManager.Setup(x => x.GetBuildingThingsQuery(It.IsAny<string>())).Returns(buildingThingsQuery);

            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(organizationQuery)).ReturnsAsync(organizationJsonDocument);
            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(siteQuery)).ReturnsAsync(siteJsonDocument);
            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(buildingThingsQuery)).ReturnsAsync(buildingThingsJsonDocument);

            var mockOutputGraphManager = new Mock<IOutputGraphManager>();

            TelemetryConfiguration appInsightsConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new Mock<ITelemetryChannel>().Object,
            };

            var telemetryClient = new TelemetryClient(appInsightsConfiguration);

            var mockOntologyMappingManager = new Mock<IOntologyMappingManager>();

            var graphNamingManager = new DefaultGraphNamingManager();

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager.Object, mockOntologyMappingManager.Object, mockOutputGraphManager.Object, graphNamingManager, telemetryClient);

            await graphIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
        }
    }
}