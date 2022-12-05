//-----------------------------------------------------------------------
// <copyright file="TargetTypeNotFoundException.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Exceptions
{
    using System;

    /// <summary>
    /// Custom exception for when there is an issue transforming telemetry value.
    /// </summary>
    public class TelemetryValueException : Exception
    {
        /// <summary>
        /// Custom exception for when there is an issue transforming telemetry value.
        /// </summary>
        /// <param name="message">In your own words, describe what happened</param>
        public TelemetryValueException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Custom exception for when there is an issue transforming telemetry value.
        /// </summary>
        /// <param name="message">In your own words, describe what happened</param>
        /// <param name="exception">If relevant add the root cause exception</param>
        public TelemetryValueException(string message, Exception exception)
            : base(message, exception)
        {
        }
    }
}
