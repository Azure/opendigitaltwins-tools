//-----------------------------------------------------------------------
// <copyright file="ITelemetryIngestionProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Interfaces
{
    using Azure.Messaging.EventHubs;

    public interface ITelemetryIngestionProcessor
    {
        /// <summary>
        /// Defines how to processes messages that flow across an eventHub
        /// </summary>
        /// <param name="telemetryEvent">A single eventHub message</param>
        /// <param name="cancellationToken">A way to stop things</param>
        Task IngestFromEventHubAsync(EventData telemetryEvent, CancellationToken cancellationToken);
    }
}