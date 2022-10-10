//-----------------------------------------------------------------------
// <copyright file="IOutputGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    using System.Threading.Tasks;
    using global::Azure.DigitalTwins.Core;

    public interface IOutputGraphManager
    {
        public Task<IEnumerable<string>> GetModelAsync(CancellationToken cancellationToken);

        public Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships, CancellationToken cancellationToken);
    }
}
