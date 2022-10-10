// -----------------------------------------------------------------------
// <copyright file="FacilitiesException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Exceptions
{
    using System;

    public class FacilitiesException : Exception
    {
        public FacilitiesException(string message)
            : base(message)
        {
        }
    }
}
