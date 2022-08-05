// -----------------------------------------------------------------------
// <copyright file="OntologyMappingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartFacilities.OntologyMapper
{
    using System.Reflection;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class OntologyMappingManager : IOntologyMappingManager
    {
        private readonly ILogger logger;
        private OntologyMapping ontologyMapping;

        public OntologyMappingManager(ILogger logger, string exceptionsFileName)
        {
            this.logger = logger;
            ontologyMapping = LoadOntologyMapping(logger, exceptionsFileName);
        }

        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap)
        {
            dtmiRemap = ontologyMapping.InterfaceRemaps.FirstOrDefault(r => r.InputDtmi == inputDtmi.ToString() && !r.IsIgnored);

            if (dtmiRemap != null)
            {
                return true;
            }

            return false;
        }

        public bool TryGetRelationshipRemapDtmi(string inputRelationship, out string outputRelationship)
        {
            outputRelationship = string.Empty;

            var result = ontologyMapping.RelationshipRemaps.FirstOrDefault(r => r.InputRelationship == inputRelationship.ToString());

            if (result != null)
            {
                outputRelationship = result.OutputRelationship;
                return true;
            }

            return false;
        }

        public bool TryGetPropertyProjection(string outputDtmiFilter, string outputPropertyName, out PropertyProjection? propertyProjection)
        {
            propertyProjection = null;

            var result = ontologyMapping.PropertyProjections.FirstOrDefault(e => e.OutputDtmiFilter == outputDtmiFilter && e.OutputPropertyName == outputPropertyName);

            if (result != null)
            {
                propertyProjection = result;
                return true;
            }

            return false;
        }

        public bool TryGetFillProperty(string outputDtmiFilter, string outputPropertyName, out IEnumerable<string> inputPropertyNames)
        {
            inputPropertyNames = new List<string>();

            var result = ontologyMapping.FillProperties.FirstOrDefault(e => e.OutputDtmiFilter == outputDtmiFilter && e.OutputPropertyName == outputPropertyName);

            if (result != null)
            {
                inputPropertyNames = result.InputPropertyNames.Split(" ").ToList();
                return true;
            }

            return false;
        }

        public bool ValidateTargetOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> targetObjectModel, out List<string> invalidTargets)
        {
            invalidTargets = new List<string>();

            foreach (var interfaceRemap in ontologyMapping.InterfaceRemaps.Where(ir => !ir.IsIgnored))
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

        private OntologyMapping LoadOntologyMapping(ILogger logger, string ontologyFileName)
        {
            logger.LogInformation("Loading Ontology Mapping file: {fileName}", ontologyFileName);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(ontologyFileName));

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
                            var error = $"Mappings file '{ontologyFileName}' is empty or poorly formed.";
                            throw new Exception(error);
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException(ontologyFileName);
                }
            }
        }
    }
}
