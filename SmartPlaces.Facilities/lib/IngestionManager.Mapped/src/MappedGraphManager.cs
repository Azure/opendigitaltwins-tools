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
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    /// <summary>
    /// Load a topology graph from a Mapped instance via the Mapped API.
    /// </summary>
    public class MappedGraphManager : IInputGraphManager
    {
        private readonly ILogger logger;
        private readonly MappedIngestionManagerOptions options;
        private readonly HttpClient httpClient;
        private readonly JsonDocument model;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedGraphManager"/> class.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger">ILogger</see> used to log status as needed.</param>
        /// <param name="httpClientFactory">An instance of <see cref="IHttpClientFactory">IHttpClientFactory</see> used to create an HttpClient.</param>
        /// <param name="options">An instance of IOptions of <see cref="MappedIngestionManagerOptions">MappedIngestionManagerOptions</see> used to pass paramters to the Graph Manager.</param>
        public MappedGraphManager(ILogger<MappedGraphManager> logger, IHttpClientFactory httpClientFactory, IOptions<MappedIngestionManagerOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;

            model = LoadObjectModelJson();

            httpClient = httpClientFactory.CreateClient("Microsoft.SmartPlaces.Facilities");
        }

        /// <summary>
        /// Generic method for getting a JsonDocument from Mapped Graph API for a passed in Graph Query.
        /// </summary>
        /// <param name="query">A formatted graph query.</param>
        /// <returns>A JSON Document containing the results of the query against the Mapped API.</returns>
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

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonDocument.Parse(response);
            }

            return null;
        }

        /// <summary>
        /// Creates a query that returns all the sites associated to an organization.
        /// </summary>
        /// <returns>A formatted graph query.</returns>
        public string GetOrganizationQuery()
        {
            return "{sites{description,exactType,id,name}}";
        }

        /// <summary>
        /// Creates a query that returns all the buildings associated to a site.
        /// </summary>
        /// <param name="siteId">Site to search.</param>
        /// <returns>A formatted graph query.</returns>
        public string GetBuildingsForSiteQuery(string siteId)
        {
            return "{ sites(filter: { id: { eq: \"" + siteId + "\"} }) { description,exactType,id,name,buildings{ description,exactType,id,name,floors{ description,exactType,id,level,name} } } }";
        }

        /// <summary>
        /// Creates a query that returns all the things associated to a building.
        /// </summary>
        /// <param name="buildingDtId">Building to search.</param>
        /// <returns>A formatted graph query.</returns>
        public string GetBuildingThingsQuery(string buildingDtId)
        {
            return "{ buildings(filter: { id: { eq: \"" + buildingDtId + "\"} }) { things{ description,exactType,firmwareVersion,id,mappingKey,name,hasLocation{ exactType,id,name},isFedBy{ id,name,exactType}} } }";
        }

        /// <summary>
        /// Creates a query that returns all the points associated to a thing.
        /// </summary>
        /// <param name="thingDtId">Thing to search.</param>
        /// <returns>A formatted graph query.</returns>
        public string GetPointsForThingQuery(string thingDtId)
        {
            return "{ things(filter: { id: { eq: \"" + thingDtId + "\" } }) { points(filter: { exactType: { ne: \"Point\"} }) { description,exactType,id,mappingKey,name} } }";
        }

        /// <summary>
        /// Creates a query that returns all the floors associated to a building.
        /// </summary>
        /// <param name="buildingDtId">Building to search.</param>
        /// <returns>A formatted graph query.</returns>
        public string GetFloorQuery(string buildingDtId)
        {
            return "{ floors(filter: { id: { eq: \"" + buildingDtId + "\"} }) { description,exactType,id,level,name,hasPart{ exactType,id,name},zones{ description,exactType,id,name} } }";
        }

        /// <summary>
        /// Try to get a Digital Twins Model Interface for a Mapped Exact type.
        /// </summary>
        /// <param name="exactType">The exact type of the twin from Mapped.</param>
        /// <param name="dtmi">The output DTMI if found, otherwise string.Empty.</param>
        /// <returns>true if found, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the exact type passed in is Null, String.Empty, or Whitespace.</exception>
        public bool TryGetDtmi(string exactType, out string dtmi)
        {
            dtmi = string.Empty;

            if (string.IsNullOrWhiteSpace(exactType))
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
