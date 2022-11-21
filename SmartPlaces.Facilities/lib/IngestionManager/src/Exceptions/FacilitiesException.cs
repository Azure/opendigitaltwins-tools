// -----------------------------------------------------------------------
// <copyright file="FacilitiesException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Exceptions
{
    using System;

    /// <summary>
    /// Exception class for unexpected behaviors in the SmartPlaces.Facilities libraries.
    /// </summary>
    public class FacilitiesException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FacilitiesException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        public FacilitiesException(string message)
            : base(message)
        {
        }
    }
}
