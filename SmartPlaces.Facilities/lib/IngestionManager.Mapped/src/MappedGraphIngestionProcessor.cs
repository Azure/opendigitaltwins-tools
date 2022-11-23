//-----------------------------------------------------------------------
// <copyright file="MappedGraphIngestionProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using global::Azure.DigitalTwins.Core;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.OntologyMapper;

    /// <summary>
    /// Loads a building graph from a Mapped input source to the target.
    /// The logic here is specific to the way Mapped stores its topology and if a new
    /// Input Graph Provider is added, this logic will likely have to be customized.
    /// </summary>
    /// <typeparam name="TOptions">Anything that inherits from the base class of IngestionManagerOptions.</typeparam>
    public class MappedGraphIngestionProcessor<TOptions> : IngestionProcessorBase<TOptions>, IGraphIngestionProcessor
        where TOptions : IngestionManagerOptions
    {
        private readonly MetricIdentifier exactTypeNotFoundMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "ExactTypeNotFound", Metrics.IdDimensionName);

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedGraphIngestionProcessor{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger">ILogger</see> used to log status as needed.</param>
        /// <param name="inputGraphManager">An instance of an <see cref="IInputGraphManager">IInputGraphManager</see> used to load a graph from the input source.</param>
        /// <param name="ontologyMappingManager">An instance of an <see cref="IOntologyMappingManager">IOntologyMappingManager</see> used to map the input ontology to the output ontology.</param>
        /// <param name="outputGraphManager">An instance of an <see cref="IOutputGraphManager">IOutputGraphManager</see> used to save a graph to the output target.</param>
        /// <param name="telemetryClient">An instance of a <see cref="TelemetryClient">telemetry client</see> used to record metrics to a metrics store.</param>
        public MappedGraphIngestionProcessor(ILogger<MappedGraphIngestionProcessor<TOptions>> logger,
                                             IInputGraphManager inputGraphManager,
                                             IOntologyMappingManager ontologyMappingManager,
                                             IOutputGraphManager outputGraphManager,
                                             TelemetryClient telemetryClient)
            : base(logger,
                   inputGraphManager,
                   ontologyMappingManager,
                   outputGraphManager,
                   telemetryClient)
        {
        }

        /// <summary>
        /// Start the ingestion process for a collection of buildings.
        /// </summary>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An awaitable task.</returns>
        protected override async Task GetSites(CancellationToken cancellationToken)
        {
            // Generate the outermost query to run against the input graph. Starts by getting the list of sites
            var metricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "SiteProcessed", Metrics.SiteDimensionName);

            var query = InputGraphManager.GetOrganizationQuery();

            // Get the input twin graph
            var inputSites = await InputGraphManager.GetTwinGraphAsync(query).ConfigureAwait(false);

            // Loop through all of the sites and process
            if (inputSites != null)
            {
                foreach (var topElement in inputSites.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var siteElement in dataElement.Value.EnumerateArray())
                        {
                            await UpdateOutputSiteAsync(siteElement, cancellationToken);
                            TelemetryClient.GetMetric(metricIdentifier).TrackValue(1, siteElement.GetProperty("name").ToString());
                        }
                    }
                }
            }
            else
            {
                Logger.LogInformation("No sites found. Please check the configuration.");
            }
        }

        private async Task UpdateOutputSiteAsync(JsonElement siteElement, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Creating Site...");

            // Create a list of all of the twins that need to be created
            var twins = new Dictionary<string, BasicDigitalTwin>();

            // Create a list of all of the relationships that need to be created
            var relationships = new Dictionary<string, BasicRelationship>();

            await GetPlacesAsync(twins, relationships, siteElement, null, null);

            var siteDtId = siteElement.GetProperty("id").ToString();

            var filteredQuery = InputGraphManager.GetBuildingsForSiteQuery(siteDtId);

            // Get the buildings for the site
            var inputBuildings = await InputGraphManager.GetTwinGraphAsync(filteredQuery).ConfigureAwait(false);

            var twinsMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "Twins", Metrics.BuildingDimensionName);
            var relationshipsMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "Relationships", Metrics.BuildingDimensionName);
            var buildingMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "Buildings", Metrics.BuildingDimensionName);

            // Loop through all of the buildings and process
            if (inputBuildings != null)
            {
                foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var sitesElement in dataElement.Value.EnumerateArray())
                        {
                            foreach (var buildingsElement in sitesElement.EnumerateObject().Where(e => e.Name == "buildings"))
                            {
                                foreach (var buildingElement in buildingsElement.Value.EnumerateArray().Where(e => e.ValueKind != JsonValueKind.Null))
                                {
                                    await GetPlacesAsync(twins, relationships, buildingElement, sitesElement, "hasPart");

                                    await GetBuildingThingsAsync(twins, relationships, buildingElement);

                                    TelemetryClient.GetMetric(twinsMetricIdentifier).TrackValue(twins.Count, buildingElement.GetProperty("name").ToString());
                                    TelemetryClient.GetMetric(relationshipsMetricIdentifier).TrackValue(relationships.Count, buildingElement.GetProperty("name").ToString());
                                    TelemetryClient.GetMetric(buildingMetricIdentifier).TrackValue(1, buildingElement.GetProperty("name").ToString());
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogInformation("No buildings found for site: '{siteDtId}'.", siteDtId);
            }

            await OutputGraphManager.UploadGraphAsync(twins, relationships, cancellationToken);

            Logger.LogInformation("Completed updating site.");
        }

        private async Task GetBuildingThingsAsync(IDictionary<string, BasicDigitalTwin> twins,
                                   IDictionary<string, BasicRelationship> relationships,
                                   JsonElement targetElement)
        {
            var basicDtId = targetElement.GetProperty("id").ToString();
            var query = InputGraphManager.GetBuildingThingsQuery(basicDtId);

            // Get the Things for the building
            var inputBuildings = await InputGraphManager.GetTwinGraphAsync(query).ConfigureAwait(false);

            // Loop through all of the buildings and process
            if (inputBuildings != null)
            {
                foreach (var topElement in inputBuildings.RootElement.EnumerateObject())
                {
                    foreach (var dataElement in topElement.Value.EnumerateObject())
                    {
                        foreach (var buildingsElement in dataElement.Value.EnumerateArray())
                        {
                            foreach (var thingsElement in buildingsElement.EnumerateObject())
                            {
                                foreach (var thingElement in thingsElement.Value.EnumerateArray())
                                {
                                    await GetThingAsync(twins, relationships, thingElement).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task GetThingAsync(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, JsonElement thingElement)
        {
            // Get the Id of the individual item in the graph
            var thingDtId = thingElement.GetProperty("id").ToString();

            thingElement.TryGetProperty("mappingKey", out var mappingKeyProperty);

            string thingMappingKey = mappingKeyProperty.ValueKind != JsonValueKind.Undefined && mappingKeyProperty.ValueKind != JsonValueKind.Null ? mappingKeyProperty.ToString() : string.Empty;

            Dtmi? thingDtmi = null;

            // Look up the Model Id from the Incoming element
            if (thingElement.TryGetProperty("exactType", out var thingExactType))
            {
                thingDtmi = GetTwin(twins, thingElement, thingDtId, thingExactType.ToString());

                // Add the relationship to the location
                var locationElement = thingElement.EnumerateObject().FirstOrDefault(t => t.Name == "hasLocation");

                if (locationElement.Value.ValueKind != JsonValueKind.Null)
                {
                    var locationId = locationElement.Value.GetProperty("id").GetString();

                    if (locationElement.Value.TryGetProperty("exactType", out var locationExactType))
                    {
                        Dtmi? locationDtmi = GetInputInterfaceDtmi(locationExactType.ToString());
                        GetRelationship(relationships, locationId, locationDtmi, "isLocationOf", thingDtId, thingExactType.ToString());
                    }
                }

                await GetPointsAsync(twins, relationships, thingDtId, thingDtmi).ConfigureAwait(false);
            }
        }

        private async Task GetPointsAsync(IDictionary<string, BasicDigitalTwin> twins, IDictionary<string, BasicRelationship> relationships, string thingDtId, Dtmi? thingDtmi)
        {
            var pointsQuery = InputGraphManager.GetPointsForThingQuery(thingDtId);

            // Get the points for the thing
            var inputThings = await InputGraphManager.GetTwinGraphAsync(pointsQuery).ConfigureAwait(false);

            // Loop through all of the buildings and process
            if (inputThings != null)
            {
                foreach (var topThingElement in inputThings.RootElement.EnumerateObject())
                {
                    foreach (var dataThingElement in topThingElement.Value.EnumerateObject())
                    {
                        foreach (var thingElement in dataThingElement.Value.EnumerateArray())
                        {
                            foreach (var pointsElement in thingElement.EnumerateObject())
                            {
                                foreach (var pointElement in pointsElement.Value.EnumerateArray())
                                {
                                    // Get the Id of the individual item in the graph
                                    var pointDtId = pointElement.GetProperty("id").ToString();

                                    // Look up the Model Id from the Incoming element
                                    if (pointElement.TryGetProperty("exactType", out var pointExactType))
                                    {
                                        GetTwin(twins, pointElement, pointDtId, pointExactType.ToString());

                                        GetRelationship(relationships, thingDtId, thingDtmi, "hasPoint", pointDtId, pointExactType.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task GetPlacesAsync(IDictionary<string, BasicDigitalTwin> twins,
                                     IDictionary<string, BasicRelationship> relationships,
                                     JsonElement targetElement,
                                     JsonElement? sourceElement,
                                     string? relationshipType)
        {
            // Get the Id of the individual item in the graph
            var targetDtId = targetElement.GetProperty("id").ToString();

            // Look up the Model Id from the Incoming element
            if (targetElement.TryGetProperty("exactType", out var targetExactType))
            {
                GetTwin(twins, targetElement, targetDtId, targetExactType.ToString());

                // Determine if the node has descendants, and if so, iterate through them to add child twins
                var elements = targetElement.EnumerateObject();

                foreach (var innerElement in elements)
                {
                    switch (innerElement.Value.ValueKind)
                    {
                        case JsonValueKind.Array:
                            foreach (var item in innerElement.Value.EnumerateArray())
                            {
                                await GetPlacesAsync(twins, relationships, item, targetElement, innerElement.Name);
                            }

                            break;

                        case JsonValueKind.Object:
                            await GetPlacesAsync(twins, relationships, innerElement.Value, targetElement, innerElement.Name);
                            break;
                    }
                }

                if (sourceElement != null && !string.IsNullOrWhiteSpace(relationshipType) && targetElement.ValueKind != JsonValueKind.Array)
                {
                    var sourceDtId = sourceElement.Value.GetProperty("id").ToString();
                    var sourceExactType = sourceElement.Value.GetProperty("exactType").ToString();
                    var sourceDtmi = GetInputInterfaceDtmi(sourceExactType);

                    GetRelationship(relationships, sourceDtId, sourceDtmi, relationshipType, targetDtId, targetExactType.ToString());
                }

                if (string.Equals(targetExactType.ToString(), "floor", StringComparison.OrdinalIgnoreCase))
                {
                    // Get sub spaces of floor
                    var filteredQuery = InputGraphManager.GetFloorQuery(targetDtId);

                    var inputFloor = await InputGraphManager.GetTwinGraphAsync(filteredQuery);

                    if (inputFloor != null)
                    {
                        // Loop through all the spaces returned from input query
                        // TODO: determine if there is a more efficient way to walk the graph
                        var rootElement = inputFloor.RootElement;

                        foreach (var topElement in rootElement.EnumerateObject())
                        {
                            foreach (var dataElement in topElement.Value.EnumerateObject())
                            {
                                foreach (var floorElement in dataElement.Value.EnumerateArray())
                                {
                                    foreach (var innerElement in floorElement.EnumerateObject())
                                    {
                                        switch (innerElement.Value.ValueKind)
                                        {
                                            case JsonValueKind.Array:
                                                foreach (var item in innerElement.Value.EnumerateArray())
                                                {
                                                    await GetPlacesAsync(twins, relationships, item, targetElement, innerElement.Name);
                                                }

                                                break;

                                            case JsonValueKind.Object:
                                                await GetPlacesAsync(twins, relationships, innerElement.Value, targetElement, innerElement.Name);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Logger.LogWarning("ExactType not found for {targetId}", targetDtId);
                TelemetryClient.GetMetric(exactTypeNotFoundMetricIdentifier).TrackValue(1, targetDtId);
            }
        }
    }
}