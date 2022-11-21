// -----------------------------------------------------------------------
// <copyright file="Metrics.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager
{
    /// <summary>
    /// Metrics metadata for Application Insights.
    /// </summary>
    internal static class Metrics
    {
#pragma warning disable SA1600 // Elements should be documented
        internal const string DefaultNamespace = "microsoft.smartplaces.facilities";
        internal const string ActionDimensionName = "Action";
        internal const string ModelIdDimensionName = "ModelId";
        internal const string StatusDimensionName = "Status";
        internal const string TwinDimensionName = "Twin";
        internal const string RelationshipTypeDimensionName = "RelationshipType";
        internal const string InterfaceTypeDimensionName = "InterfaceType";
        internal const string OutputDtmiTypeDimensionName = "OutputDtmi";
        internal const string InputDtmiTypeDimensionName = "InputDtmi";

        internal const string SucceededStatusDimension = "Succeeded";
        internal const string FailedStatusDimension = "Failed";
        internal const string ThrottledStatusDimension = "Throttled";
        internal const string SkippedStatusDimension = "Skipped";

        internal const string CreateActionDimension = "Create";
        internal const string UpdateActionDimension = "Update";
#pragma warning restore SA1600 // Elements should be documented
    }
}
