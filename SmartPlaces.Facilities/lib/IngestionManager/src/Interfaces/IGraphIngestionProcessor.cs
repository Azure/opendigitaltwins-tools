//-----------------------------------------------------------------------
// <copyright file="IGraphIngestionProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces
{
    public interface IGraphIngestionProcessor
    {
        Task IngestFromApiAsync(CancellationToken cancellationToken);
    }
}