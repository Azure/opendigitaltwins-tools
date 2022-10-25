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

    public class OntologyMappingManager : IOntologyMappingManager
    {
        public OntologyMappingManager(IOntologyMappingLoader mappingLoader)
        {
            OntologyMapping = mappingLoader.LoadOntologyMapping();
        }

        public OntologyMapping OntologyMapping { get; }

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

        public bool TryGetRelationshipRemap(string inputRelationship, out RelationshipRemap? relationshipRemap)
        {
            relationshipRemap = OntologyMapping.RelationshipRemaps.FirstOrDefault(r => r.InputRelationship == inputRelationship);

            if (relationshipRemap != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetPropertyProjection(string outputDtmi, string outputPropertyName, out PropertyProjection? propertyProjection)
        {
            propertyProjection = OntologyMapping.PropertyProjections.OrderBy(e => e.Priority).FirstOrDefault(e => e.OutputPropertyName == outputPropertyName && Regex.Match(outputDtmi, e.OutputDtmiFilter, RegexOptions.IgnoreCase).Success);

            if (propertyProjection != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetFillProperty(string outputDtmi, string outputPropertyName, out FillProperty? fillProperty)
        {
            fillProperty = OntologyMapping.FillProperties.OrderBy(e => e.Priority).FirstOrDefault(e => e.OutputPropertyName == outputPropertyName && Regex.Match(outputDtmi, e.OutputDtmiFilter, RegexOptions.IgnoreCase).Success);

            if (fillProperty != null)
            {
                return true;
            }

            return false;
        }

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
