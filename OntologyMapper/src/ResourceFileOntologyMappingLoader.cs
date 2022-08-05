// -----------------------------------------------------------------------
// <copyright file="ResourceFileOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Reflection;

    public class ResourceFileOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string resourcePath = string.Empty;

        public ResourceFileOntologyMappingLoader(ILogger logger, string resourcePath)
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
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourcePath));

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        var mappings = JsonConvert.DeserializeObject<OntologyMapping>(result);

                        if (mappings != null)
                        {
                            return mappings;
                        }
                        else
                        {
                            var error = $"Mappings file '{resourcePath}' is empty or poorly formed.";
                            throw new Exception(error);
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
