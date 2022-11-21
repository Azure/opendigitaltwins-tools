//-----------------------------------------------------------------------
// <copyright file="IGraphIngestionProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    /// <summary>
    /// Methods for ingesting a graph from a source graph and inserting into a target graph.
    /// </summary>
    public interface IGraphIngestionProcessor
    {
        /// <summary>
        /// Starts the asynchronous ingestion process from a source to a target graph. The implementation of the derived class is responsible for exposing its configuration through its constructor.
        /// </summary>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the ingestion process.</param>
        /// <returns>An awaitable task.</returns>
        Task IngestFromApiAsync(CancellationToken cancellationToken);
    }
}