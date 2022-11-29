// -----------------------------------------------------------------------
// <copyright file="FileOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    public class FileOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string filePath = string.Empty;

        public FileOntologyMappingLoader(ILogger logger, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.logger = logger;
            this.filePath = filePath;
        }

        public OntologyMapping LoadOntologyMapping()
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", filePath);

            var file = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            OntologyMapping? mappings;
            try
            {
                mappings = JsonSerializer.Deserialize<OntologyMapping>(file, options);
            }
            catch (JsonException jex)
            {
                throw new MappingFileException($"Mappings file '{filePath}' is malformed.", filePath, jex);
            }

            if (mappings == null)
            {
                throw new MappingFileException($"Mappings file '{filePath}' is empty.", filePath);
            }

            return mappings;
        }
    }
}
