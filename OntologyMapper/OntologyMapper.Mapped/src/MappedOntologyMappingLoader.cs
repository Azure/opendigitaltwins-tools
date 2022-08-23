// -----------------------------------------------------------------------
// <copyright file="MappedOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System.Reflection;

    public class MappedOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string resourcePath = string.Empty;

        public MappedOntologyMappingLoader(ILogger logger, string resourcePath)
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
