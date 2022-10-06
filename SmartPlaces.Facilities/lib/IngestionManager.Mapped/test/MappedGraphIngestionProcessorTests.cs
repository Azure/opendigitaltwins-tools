// -----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessorTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManagerManager.Mapped.Test
{
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

        public MappedGraphIngestionProcessorTests(ITestOutputHelper output)
        {
            this.output = output;
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

            var graphIngestionProcessor = new MappedGraphIngestionProcessor<IngestionManagerOptions>(mockLogger.Object, mockInputGraphManager.Object, mockOntologyMappingManager.Object, mockOutputGraphManager.Object, telemetryClient);

            await graphIngestionProcessor.IngestFromApiAsync(CancellationToken.None);
        }
    }
}