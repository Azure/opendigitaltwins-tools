// -----------------------------------------------------------------------
// <copyright file="IOntologyMappingManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Aspen.OntologyMapper
{
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Azure.DigitalTwins.Parser.Models;

    public interface IOntologyMappingManager
    {
        public bool ValidateTargetOntologyMapping(IReadOnlyDictionary<Dtmi, DTEntityInfo> targetObjectModel, out List<string> invalidTargets);

        public bool TryGetInterfaceRemapDtmi(Dtmi inputDtmi, out DtmiRemap? dtmiRemap);

        public bool TryGetRelationshipRemapDtmi(string inputRelationship, out string outputRelationship);

        public bool TryGetFillProperty(string outputDtmiFilter, string outputPropertyName, out IEnumerable<string> inputPropertyNames);

        public bool TryGetPropertyProjection(string outputDtmiFilter, string outputPropertyName, out PropertyProjection? inputProperty);
    }
}
