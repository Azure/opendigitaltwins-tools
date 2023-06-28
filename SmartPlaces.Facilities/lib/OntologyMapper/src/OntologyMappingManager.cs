// -----------------------------------------------------------------------
// <copyright file="OntologyMappingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using System.Text.RegularExpressions;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;

    /// <summary>
    /// Implements methods for consuming ontology mappings, i.e., to fetch ontology names (DTMIs, relationship names,
    /// properties, etc.) in an output ontology, that correspond to some sought names in an input ontology.
    /// </summary>
    public class OntologyMappingManager : IOntologyMappingManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OntologyMappingManager"/> class.
        /// </summary>
        /// <param name="mappingLoader">Loader that can provide a set of ontology mappings for this manager to operate over.</param>
        public OntologyMappingManager(IOntologyMappingLoader mappingLoader)
        {
            OntologyMapping = mappingLoader.LoadOntologyMapping();
        }

        /// <summary>
        /// Gets the loaded ontology mappings.
        /// </summary>
        public OntologyMapping OntologyMapping { get; }

        /// <inheritdoc/>
        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap)
        {
            dtmiRemap = OntologyMapping.InterfaceRemaps.FirstOrDefault(r => r.InputDtmi == inputDtmi.ToString() && !r.IsIgnored);

            if (dtmiRemap != null)
            {
                return true;
            }

            var namespaceRemap = OntologyMapping.NamespaceRemaps.FirstOrDefault(n => inputDtmi.ToString().Contains(n.InputNamespace));

            if (namespaceRemap != null)
            {
                dtmiRemap = new DtmiRemap()
                {
                    InputDtmi = inputDtmi.ToString(),
                    OutputDtmi = inputDtmi.ToString().Replace(namespaceRemap.InputNamespace, namespaceRemap.OutputNamespace),
                };

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryGetRelationshipRemap(string inputRelationship, out RelationshipRemap? relationshipRemap)
        {
            relationshipRemap = OntologyMapping.RelationshipRemaps.FirstOrDefault(r => r.InputRelationship == inputRelationship);

            if (relationshipRemap != null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryGetPropertyProjection(string outputDtmi, string outputPropertyName, out PropertyProjection? propertyProjection)
        {
            propertyProjection = OntologyMapping.PropertyProjections.OrderBy(e => e.Priority).FirstOrDefault(e => e.OutputPropertyName == outputPropertyName && Regex.Match(outputDtmi, e.OutputDtmiFilter, RegexOptions.IgnoreCase).Success);

            if (propertyProjection != null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryGetFillProperty(string outputDtmi, string outputPropertyName, out FillProperty? fillProperty)
        {
            fillProperty = OntologyMapping.FillProperties.OrderBy(e => e.Priority).FirstOrDefault(e => e.OutputPropertyName == outputPropertyName && Regex.Match(outputDtmi, e.OutputDtmiFilter, RegexOptions.IgnoreCase).Success);

            if (fillProperty != null)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public ObjectTransformationMatch TryGetObjectTransformation(string outputDtmi, string outputPropertyName, out ObjectTransformation? objectTransformation)
        {
            var doesPropertyHaveTransformation = OntologyMapping.ObjectTransformations.Any(e => e.OutputPropertyName == outputPropertyName);

            if (!doesPropertyHaveTransformation)
            {
                objectTransformation = null;
                return ObjectTransformationMatch.NoPropertyMatch;
            }

            objectTransformation = OntologyMapping.ObjectTransformations.OrderBy(e => e.Priority).FirstOrDefault(e => e.OutputPropertyName == outputPropertyName && Regex.Match(outputDtmi, e.OutputDtmiFilter, RegexOptions.IgnoreCase).Success);

            if (objectTransformation != null)
            {
                return ObjectTransformationMatch.PropertyAndTypeMatch;
            }

            return ObjectTransformationMatch.PropertyMatchOnly;
        }

        /// <inheritdoc/>
        public bool ValidateTargetOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> targetObjectModel, out List<string> invalidTargets)
        {
            invalidTargets = new List<string>();

            foreach (var interfaceRemap in OntologyMapping.InterfaceRemaps.Where(ir => !ir.IsIgnored))
            {
                try
                {
                    var outputDtmi = new Dtmi(interfaceRemap.OutputDtmi);
                    if (!targetObjectModel.TryGetValue(outputDtmi, out var dTEntityInfo))
                    {
                        invalidTargets.Add(interfaceRemap.OutputDtmi);
                    }
                }
                catch
                {
                    invalidTargets.Add(interfaceRemap.OutputDtmi);
                }
            }

            return !invalidTargets.Any();
        }

        /// <inheritdoc/>
        public bool ValidateSourceOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> sourceObjectModel, out List<string> invalidSources)
        {
            invalidSources = new List<string>();

            foreach (var interfaceRemap in OntologyMapping.InterfaceRemaps.Where(ir => !ir.IsIgnored))
            {
                try
                {
                    var inputDtmi = new Dtmi(interfaceRemap.InputDtmi);
                    if (!sourceObjectModel.TryGetValue(inputDtmi, out var dTEntityInfo))
                    {
                        invalidSources.Add(interfaceRemap.InputDtmi);
                    }
                }
                catch
                {
                    invalidSources.Add(interfaceRemap.InputDtmi);
                }
            }

            return !invalidSources.Any();
        }
    }
}
