//-----------------------------------------------------------------------
// <copyright file="IOutputGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    using System.Threading.Tasks;
    using global::Azure.DigitalTwins.Core;

    /// <summary>
    /// Methods for working with an output graph.
    /// </summary>
    public interface IOutputGraphManager
    {
        /// <summary>
        /// Asynchronously loads the model for a graph.
        /// </summary>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>Task wrapping a collection of strings which describe the model used by the output graph.</returns>
        public Task<IEnumerable<string>> GetModelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously loads the twins and relationships into an output graph.
        /// </summary>
        /// <param name="twins">Twins to be loaded into output graph.</param>
        /// <param name="relationships">Relationships to be loaded into output graph.</param>
        /// <param name="cancellationToken">Cancellation propagation token for interrupting the loading process.</param>
        /// <returns>An awaitable task.</returns>
        public Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships, CancellationToken cancellationToken);
    }
}
