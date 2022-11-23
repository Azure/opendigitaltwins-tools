// -----------------------------------------------------------------------
// <copyright file="FileOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Ontology mapping loader implementation that loads mappings from a file on disk; the path of
    /// which is input via class constructor.
    /// </summary>
    public class FileOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string filePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileOntologyMappingLoader"/> class.
        /// </summary>
        /// <param name="logger">Logging implementation.</param>
        /// <param name="filePath">Path to ontology mappings file.</param>
        public FileOntologyMappingLoader(ILogger logger, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            this.logger = logger;
            this.filePath = filePath;
        }

        /// <summary>
        /// Loads a set of ontology mappings.
        /// </summary>
        /// <returns>An OntologyMapping object holding a set of defined mappings (class mappings, relationship mappings, etc).</returns>
        public OntologyMapping LoadOntologyMapping()
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", filePath);

            var file = File.ReadAllText(filePath);

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            var mappings = JsonSerializer.Deserialize<OntologyMapping>(file, options);
            if (mappings != null)
            {
                return mappings;
            }
            else
            {
                throw new MappingFileException("Mappings file is empty or poorly formed.", filePath);
            }
        }
    }
}
