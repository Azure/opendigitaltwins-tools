// -----------------------------------------------------------------------
// <copyright file="OntologyMappingTests.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper.Test
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.SmartFacilities.OntologyMapper;
    using Moq;
    using System.Reflection;
    using Xunit;
    using Xunit.Abstractions;

    public class BrickRecMappingValidationTests
    {
        private readonly ITestOutputHelper output;

        public BrickRecMappingValidationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("Mappings.Mapped.Json.v0.BrickRec.mapped_json_v0_dtdlv2_Brick_1_3-REC_4_0.json")]
        [InlineData("Mappings.Mapped.Json.v0.BrickRec.mapped_json_v0_dtdlv3_Brick_1_3-REC_4_0.json")]
        public void ValidateEmbeddedResourceDtmisAreValidFormat(string resourcePath)
        {
            var mockLogger = new Mock<ILogger>();
            var resourceLoader = new EmbeddedResourceOntologyMappingLoader(mockLogger.Object, resourcePath);
            var ontologyMappingManager = new OntologyMappingManager(resourceLoader);

            var exceptions = new List<string>();
            foreach (var mapping in ontologyMappingManager.OntologyMapping.InterfaceRemaps)
            {
                try
                {
                    var inputDtmi = new Dtmi(mapping.InputDtmi);
                }
                catch (ParsingException)
                {
                    exceptions.Add($"Invalid input DTMI: {mapping.InputDtmi}");
                }

                try
                {
                    var outputDtmi = new Dtmi(mapping.OutputDtmi);
                }
                catch (ParsingException)
                {
                    exceptions.Add($"Invalid output DTMI: {mapping.OutputDtmi}");
                }
            }

            // Verify that the Interface Remaps are unique for an input interface
            foreach (var interfaceRemap in ontologyMappingManager.OntologyMapping.InterfaceRemaps)
            {
                var matchingRemapsCount = ontologyMappingManager.OntologyMapping.InterfaceRemaps.Count(p => p.InputDtmi == interfaceRemap.InputDtmi);
                if (matchingRemapsCount > 1)
                {
                    exceptions.Add($"Duplicate InterfaceRemap: {interfaceRemap.InputDtmi}");
                }
            }

            // Verify that the Interface Remaps are unique for an input interface
            foreach (var relationshipRemap in ontologyMappingManager.OntologyMapping.RelationshipRemaps)
            {
                var matchingRemapsCount = ontologyMappingManager.OntologyMapping.RelationshipRemaps.Count(p => p.InputRelationship == relationshipRemap.InputRelationship);
                if (matchingRemapsCount > 1)
                {
                    exceptions.Add($"Duplicate RelationshipRemap: {relationshipRemap.InputRelationship}");
                }
            }

            // Verify that the property projections are unique for an output property
            foreach (var projection in ontologyMappingManager.OntologyMapping.PropertyProjections)
            {
                var matchingProjectionsCount = ontologyMappingManager.OntologyMapping.PropertyProjections.Count(p => p.OutputPropertyName == projection.OutputPropertyName);
                if (matchingProjectionsCount > 1)
                {
                    exceptions.Add($"Duplicate PropertyProjection: {projection.OutputPropertyName}");
                }
            }

            // Verify that the fill properties are unique for an output property
            foreach (var fillProperty in ontologyMappingManager.OntologyMapping.FillProperties)
            {
                var matchingFillPropertyCount = ontologyMappingManager.OntologyMapping.FillProperties.Count(p => p.OutputPropertyName == fillProperty.OutputPropertyName);
                if (matchingFillPropertyCount > 1)
                {
                    exceptions.Add($"Duplicate FillProperty: {fillProperty.OutputPropertyName}");
                }
            }

            Assert.Empty(exceptions);
        }
    }
}