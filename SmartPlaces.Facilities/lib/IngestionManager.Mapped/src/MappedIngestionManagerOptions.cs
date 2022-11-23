//-----------------------------------------------------------------------
// <copyright file="MappedIngestionManagerOptions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    /// <summary>
    /// Configuration settings needed for connecting to an instance of the Mapped Graph API.
    /// </summary>
    public class MappedIngestionManagerOptions : IngestionManagerOptions, IInputGraphManagerOptions
    {
        /// <summary>
        /// Gets or sets an access token used for authorizing against the Mapped Graph API.
        /// </summary>
        [Required]
        public string MappedToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the Uri used for connecting to the Mapped Graph API.
        /// </summary>
        [Required]
        public string MappedRootUrl { get; set; } = string.Empty;
    }
}
