// -----------------------------------------------------------------------
// <copyright file="Metrics.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped
{
    /// <summary>
    /// Metrics metadata for Application Insights.
    /// </summary>
    internal static class Metrics
    {
#pragma warning disable SA1600 // Elements should be documented
        internal const string DefaultNamespace = "Mapped";
        internal const string IdDimensionName = "Id";
        internal const string SiteDimensionName = "Site";
        internal const string BuildingDimensionName = "Building";
        internal const string IsSuccessDimensionName = "IsSuccess";
#pragma warning restore SA1600 // Elements should be documented
    }
}
