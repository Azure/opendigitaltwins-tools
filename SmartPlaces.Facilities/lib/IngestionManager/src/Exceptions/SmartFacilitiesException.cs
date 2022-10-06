// -----------------------------------------------------------------------
// <copyright file="SmartFacilitiesException.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Exceptions
{
    using System;

    public class SmartFacilitiesException : Exception
    {
        public SmartFacilitiesException(string message)
            : base(message)
        {
        }
    }
}
