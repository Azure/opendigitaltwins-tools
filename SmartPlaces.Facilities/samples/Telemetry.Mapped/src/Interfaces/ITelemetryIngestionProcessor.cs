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
        Task IngestFromEventHubAsync(EventData telemetryEvent, CancellationToken cancellationToken);
    }
}