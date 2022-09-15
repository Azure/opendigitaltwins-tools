// -----------------------------------------------------------------------
// <copyright file="OntologyMappingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.OntologyMapper
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;

    public class OntologyMappingManager : IOntologyMappingManager
    {
        public OntologyMapping OntologyMapping { get; }

        public OntologyMappingManager(IOntologyMappingLoader mappingLoader)
        {
            OntologyMapping = mappingLoader.LoadOntologyMapping();
        }

        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap)
        {
            dtmiRemap = OntologyMapping.InterfaceRemaps.FirstOrDefault(r => r.InputDtmi == inputDtmi.ToString() && !r.IsIgnored);

            if (dtmiRemap != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetRelationshipRemap(string inputRelationship, out RelationshipRemap? outputRelationship)
        {
            outputRelationship = OntologyMapping.RelationshipRemaps.FirstOrDefault(r => r.InputRelationship == inputRelationship);

            if (outputRelationship != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetPropertyProjection(string outputDtmiFilter, string outputPropertyName, out PropertyProjection? propertyProjection)
        {
            propertyProjection = OntologyMapping.PropertyProjections.FirstOrDefault(e => e.OutputDtmiFilter == outputDtmiFilter && e.OutputPropertyName == outputPropertyName);

            if (propertyProjection != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetFillProperty(string outputDtmiFilter, string outputPropertyName, out FillProperty? inputPropertyNames)
        {
            inputPropertyNames = OntologyMapping.FillProperties.FirstOrDefault(e => e.OutputDtmiFilter == outputDtmiFilter && e.OutputPropertyName == outputPropertyName);

            if (inputPropertyNames != null)
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
    }
}
