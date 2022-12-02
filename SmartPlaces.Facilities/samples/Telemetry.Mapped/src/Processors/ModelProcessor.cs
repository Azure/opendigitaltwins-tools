//-----------------------------------------------------------------------
// <copyright file="ModelProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Processors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure.DigitalTwins.Core;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using Telemetry.Exceptions;

    internal static class ModelProcessor
    {
        private static readonly ModelParser modelParser = new ModelParser();

        /// <summary>
        /// Reaches out to AzureDigitalTwins to determine what the propertyType is for a given
        /// modelId and property field. This method will search up the dependency tree for 
        /// specified models.
        /// </summary>
        /// <param name="adt">Connection to an AzureDigitalTwins instance.</param>
        /// <param name="modelId">A full DTDL ModelId to search.</param>
        /// <param name="property">Substring of the AbsoluteUri of the property to search the given model(s) for.</param>
        /// <param name="cancellationToken">A way to stop things.</param>
        /// <returns>The target datatype of the property.</returns>
        /// <exception cref="TargetTypeNotFoundException">Thrown when the given modelId does not contain the given property.</exception>
        public static async Task<DTEntityKind> GetEntityKindFromModelIdAsync(DigitalTwinsClient adt, string modelId, string property, CancellationToken cancellationToken = default)
        {
            var response = adt.GetModelsAsync(new GetModelsOptions() { IncludeModelDefinition = true, DependenciesFor = new[] { modelId } }, cancellationToken);
            var models = new List<string>();
            await foreach (var digitalTwinsModelData in response)
            {
                models.Add(digitalTwinsModelData.DtdlModel);
            }

            var parseResult = await modelParser.ParseAsync(models);

            // The Value needs cast to get to its Schema EntityKind
            var dtEntityInfo = (DTFieldInfo)parseResult.Where(x => x.Key.AbsoluteUri.Contains(property)).FirstOrDefault().Value;

            if(dtEntityInfo?.Schema?.EntityKind is null)
            {
                throw new TargetTypeNotFoundException($"Failed to find target type for {property} on {modelId}");
            }

            return dtEntityInfo.Schema.EntityKind;
        }
    }
}
