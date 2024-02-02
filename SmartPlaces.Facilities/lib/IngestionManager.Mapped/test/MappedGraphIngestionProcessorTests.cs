// -----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessorTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped.Test
{
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
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

    public class MappedGraphIngestionProcessorTests
    {

        private const string organizationQuery = "{sites{description,exactType,id,name}}";
        private const string siteQuery = "{ sites(filter: { id: { eq: \"SITE44nUdjjbqSX1sEXuwEWucr\"} }) { description,exactType,id,name,dateCreated,dateUpdated,type,buildings{ address{countryName,dateCreated,dateUpdated,id,locality,postalCode,region,streetAddress},description,exactType,id,identities { ... on ExternalIdentity { dateCreated, dateUpdated, value } },name,type,floors{ dateCreated,dateUpdated,description,exactType,id,level,name,type} } } }";
        private const string buildingThingsQuery = "{ buildings(filter: { id: { eq: \"BLDG5o26DguWKu5T9nRvSYn5Em\"} }) { things{ dateCreated,dateUpdated,description,exactType,firmwareVersion,id,mappingKey,model { id,description,manufacturer { id,name,description,logoUrl }, manufacturerId,name,imageUrl,seeAlsoUrls },name,hasLocation{ exactType,id,name },isFedBy{ id,name,exactType }} } }";
        private const string thingPointsQuery = "{ things(filter: { id: { eq: \"THGKVAKMMkuZ7LRYeqn2voGhg\" } }) { points(filter: { exactType: { ne: \"Point\"} }) { dateCreated,dateUpdated,description,exactType,id,mappingKey,name,unit{description,id,name} } }";

        private readonly JsonDocument? siteJsonDocument;
        private readonly JsonDocument? organizationJsonDocument;
        private readonly JsonDocument? buildingThingsJsonDocument;
        private readonly JsonDocument? thingPointsJsonDocument;

        public MappedGraphIngestionProcessorTests()
        {
            organizationJsonDocument = GetDocumentFromResource("organization.json");
            siteJsonDocument = GetDocumentFromResource("site.json");
            buildingThingsJsonDocument = GetDocumentFromResource("buildingThings.json");
            thingPointsJsonDocument = GetDocumentFromResource("thingPoints.json");
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
            mockInputGraphManager.Setup(x => x.GetPointsForThingQuery(It.IsAny<string>())).Returns(thingPointsQuery);

            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(organizationQuery)).ReturnsAsync(organizationJsonDocument);
            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(siteQuery)).ReturnsAsync(siteJsonDocument);
            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(buildingThingsQuery)).ReturnsAsync(buildingThingsJsonDocument);
            mockInputGraphManager.Setup(x => x.GetTwinGraphAsync(thingPointsQuery)).ReturnsAsync(thingPointsJsonDocument);

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

        private static JsonDocument? GetDocumentFromResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));
            var jsonDocument = null as JsonDocument;
            using (Stream? stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        var organizationReader = new Utf8JsonReader(Encoding.UTF8.GetBytes(result));
                        _ = JsonDocument.TryParseValue(ref organizationReader, out jsonDocument);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }

            return jsonDocument;
        }
    }
}