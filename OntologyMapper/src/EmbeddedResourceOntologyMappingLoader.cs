// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using Microsoft.Extensions.Logging;
    using System.Reflection;
    using System.Text.Json;

    public class EmbeddedResourceOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string resourcePath = string.Empty;

        public EmbeddedResourceOntologyMappingLoader(ILogger logger, string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            this.logger = logger;
            this.resourcePath = resourcePath;
        }

        public OntologyMapping LoadOntologyMapping()
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", resourcePath);

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.Single(str => str.ToLowerInvariant().EndsWith(resourcePath.ToLowerInvariant()));

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        var mappings = JsonSerializer.Deserialize<OntologyMapping>(result, options);

                        if (mappings != null)
                        {
                            return mappings;
                        }
                        else
                        {
                            throw new MappingFileException("Mappings file is empty or poorly formed.", resourcePath);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(resourcePath);
                }
            }
        }
    }
}
