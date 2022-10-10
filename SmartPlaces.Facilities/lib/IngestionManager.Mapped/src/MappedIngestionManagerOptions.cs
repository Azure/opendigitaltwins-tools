//-----------------------------------------------------------------------
// <copyright file="MappedIngestionManagerOptions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped
{
    using System.ComponentModel.DataAnnotations;
    using IngestionManager.Interfaces;

    public class MappedIngestionManagerOptions : IngestionManagerOptions, IInputGraphManagerOptions
    {
        [Required]
        public string MappedToken { get; set; } = string.Empty;

        [Required]
        public string MappedRootUrl { get; set; } = string.Empty;
    }
}
