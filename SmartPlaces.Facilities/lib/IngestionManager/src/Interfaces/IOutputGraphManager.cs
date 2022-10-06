//-----------------------------------------------------------------------
// <copyright file="IOutputGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IngestionManager.Interfaces
{
    using System.Threading.Tasks;
    using Azure.DigitalTwins.Core;

    public interface IOutputGraphManager
    {
        public Task<IEnumerable<string>> GetModelAsync(CancellationToken cancellationToken);

        public Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships, CancellationToken cancellationToken);
    }
}
