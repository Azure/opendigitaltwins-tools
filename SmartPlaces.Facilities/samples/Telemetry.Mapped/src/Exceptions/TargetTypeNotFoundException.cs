//-----------------------------------------------------------------------
// <copyright file="TargetTypeNotFoundException.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception for when the DTDL Model has not been defined
    /// for the incoming telemetry. 
    /// </summary>
    public class TargetTypeNotFoundException : Exception
    {
        /// <summary>
        /// Custom exception for when the DTDL Model has not been defined
        /// for the incoming telemetry. 
        /// </summary>
        /// <param name="message">In your own words, describe what happened</param>
        public TargetTypeNotFoundException(string message)
            : base(message)
        {
        }
    }
}
