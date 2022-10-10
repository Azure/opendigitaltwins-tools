//-----------------------------------------------------------------------
// <copyright file="MappedGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped
{
    using System.Net.Http.Json;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using mapped;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    public class MappedGraphManager : IInputGraphManager
    {
        private readonly ILogger logger;
        private readonly MappedIngestionManagerOptions options;
        private readonly HttpClient httpClient;
        private readonly JsonDocument model;

        public MappedGraphManager(ILogger<MappedGraphManager> logger, IHttpClientFactory httpClientFactory, IOptions<MappedIngestionManagerOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            model = LoadObjectModelJson();

            httpClient = httpClientFactory.CreateClient("Microsoft.SmartPlaces.Facilities");
        }

        public async Task<JsonDocument?> GetTwinGraphAsync(string query)
        {
            logger.LogInformation("Getting topology from mapped. {query}", query);

            var queryObject = new
            {
                query = "query " + query,
            };

            var httpRequestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, options.MappedRootUrl)
            {
                Headers =
                {
                    { HeaderNames.Accept, "application/json" },
                },
                Content = JsonContent.Create(queryObject),
            };

            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", options.MappedToken);

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            // TODO: jobee - Improve error handling here
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonDocument.Parse(response);
            }

            return null;
        }

        public string GetOrganizationQuery()
        {
            var builder = new OrgQueryBuilder()
                    .WithSites(new SiteQueryBuilder()
                        .WithAllScalarFields());

            return builder.Build(Formatting.None);
        }

        public string GetBuildingsForSiteQuery(string siteId)
        {
            // Get the buildings for the site
            var builder = new SiteQueryBuilder()
                .WithAllScalarFields()
                .WithBuildings(new BuildingQueryBuilder()
                                    .WithAllScalarFields()
                                    .WithFloors(new FloorQueryBuilder()
                                        .WithAllScalarFields()));

            var query = builder.Build(Formatting.None);
            var filteredQuery = "{sites(filter: {id: {eq: \"" + siteId + "\"}}) " + query + "}";

            return filteredQuery;
        }

        public string GetBuildingThingsQuery(string buildingDtId)
        {
            var builder = new BuildingQueryBuilder()
                .WithThings(new ThingQueryBuilder()
                    .WithAllScalarFields()
                    .WithHasLocation(new PlaceQueryBuilder()
                                       .WithAllScalarFields()));

            var query = builder.Build(Formatting.None);
            var filteredQuery = "{buildings(filter: {id: {eq: \"" + buildingDtId + "\"}}) " + query + "}";

            return filteredQuery;
        }

        public string GetPointsForThingQuery(string thingDtId)
        {
            var builder = new ThingQueryBuilder()
                .WithPoints(new PointQueryBuilder()
                    .WithAllScalarFields());

            var query = builder.Build(Formatting.None);
            var filteredQuery = "{things(filter: {id: {eq: \"" + thingDtId + "\"}}) " + query + "}";

            return filteredQuery;
        }

        public string GetFloorQuery(string basicDtId)
        {
            var builder = new FloorQueryBuilder()
                .WithAllScalarFields()
                .WithHasPart(new PlaceQueryBuilder()
                   .WithAllScalarFields())
                .WithZones(new ZoneQueryBuilder()
                    .WithAllScalarFields());

            var query = builder.Build();
            var filteredQuery = "{floors(filter: {id: {eq: \"" + basicDtId + "\"}}) " + query + "}";

            return filteredQuery;
        }

        public bool TryGetDtmi(string exactType, out string dtmi)
        {
            dtmi = string.Empty;

            if (string.IsNullOrEmpty(exactType))
            {
                throw new ArgumentNullException(nameof(exactType));
            }

            try
            {
                var root = model.RootElement.EnumerateArray();

                var element = root.FirstOrDefault(e => e.TryGetProperty("displayName", out var propertyName) && string.Compare(propertyName.ToString(), exactType, StringComparison.OrdinalIgnoreCase) == 0);
                if (element.ValueKind != JsonValueKind.Null && element.TryGetProperty("@id", out var idProperty))
                {
                    dtmi = idProperty.ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting DTMI from Mapped DTDL for ExactType: '{exactType}'", exactType);
                return false;
            }

            return false;
        }

        private static JsonDocument LoadObjectModelJson()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("mapped_dtdl.json"));

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        return JsonDocument.Parse(result);
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourceName);
                }
            }
        }
    }
}
