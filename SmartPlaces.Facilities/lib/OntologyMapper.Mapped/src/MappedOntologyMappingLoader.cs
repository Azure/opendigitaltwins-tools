// -----------------------------------------------------------------------
// <copyright file="MappedOntologyMappingLoader.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper.Mapped
{
    using System.Reflection;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Ontology mapping loader implementation that loads mappings from a
    /// resource file embedded within the assembly.
    /// </summary>
    public class MappedOntologyMappingLoader : IOntologyMappingLoader
    {
        private readonly ILogger logger;
        private readonly string resourcePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappedOntologyMappingLoader"/> class.
        /// </summary>
        /// <param name="logger">Logging implementation.</param>
        /// <param name="resourcePath">Path to ontology mappings file embedded within assembly, using dot notation, e.g., <em>Mappings.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json</em>.</param>
        public MappedOntologyMappingLoader(ILogger logger, string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            this.logger = logger;
            this.resourcePath = resourcePath;
        }

        /// <inheritdoc/>
        public OntologyMapping LoadOntologyMapping()
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", resourcePath);

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var resourceName = resources.Single(str => str.ToLowerInvariant().EndsWith(resourcePath.ToLowerInvariant()));

            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string result = reader.ReadToEnd();
                        OntologyMapping? mappings;
                        try
                        {
                            mappings = JsonSerializer.Deserialize<OntologyMapping>(result, options);
                        }
                        catch (JsonException jex)
                        {
                            throw new MappingFileException($"Mappings file '{resourcePath}' is malformed.", resourcePath, jex);
                        }

                        if (mappings == null)
                        {
                            throw new MappingFileException($"Mappings file '{resourcePath}' is empty.", resourcePath);
                        }

                        return mappings;
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
