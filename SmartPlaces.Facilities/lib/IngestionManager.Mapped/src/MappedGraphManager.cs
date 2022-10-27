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
            return "{sites{description,exactType,id,name}}";
        }

        public string GetBuildingsForSiteQuery(string siteId)
        {
            return "{ sites(filter: { id: { eq: \"" + siteId + "\"} }) { description,exactType,id,name,buildings{ description,exactType,id,name,floors{ description,exactType,id,level,name} } } }";
        }

        public string GetBuildingThingsQuery(string buildingDtId)
        {
            return "{ buildings(filter: { id: { eq: \"" + buildingDtId + "\"} }) { things{ description,exactType,firmwareVersion,id,mappingKey,name,hasLocation{ exactType,id,name} } } }";
        }

        public string GetPointsForThingQuery(string thingDtId)
        {
            return "{ things(filter: { id: { eq: \"" + thingDtId + "\" } }) { points(filter: { exactType: { ne: \"Point\"} }) { description,exactType,id,mappingKey,name} } }";
        }

        public string GetFloorQuery(string basicDtId)
        {
            return "{ floors(filter: { id: { eq: \"" + basicDtId + "\"} }) { description,exactType,id,level,name,hasPart{ exactType,id,name},zones{ description,exactType,id,name} } }";
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
