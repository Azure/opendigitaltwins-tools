// -----------------------------------------------------------------------
// <copyright file="EventHubProcessor.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Telemetry.Processors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Azure;
    using global::Azure.Core;
    using global::Azure.Messaging.EventHubs;
    using global::Azure.Messaging.EventHubs.Consumer;
    using global::Azure.Messaging.EventHubs.Primitives;
    using global::Azure.Storage.Blobs;
    using global::Azure.Storage.Blobs.Models;
    using Microsoft.Extensions.Logging;

    public class EventHubProcessor<TPartition> : EventProcessor<TPartition>
        where TPartition : EventProcessorPartition, new()
    {
        private readonly ILogger logger;
        private readonly BlobContainerClient storageContainer;
        private readonly Func<EventData, CancellationToken, Task> processEventData;

        private const string OwnershipPrefixFormat = "{0}/{1}/{2}/ownership/";
        private const string OwnerIdentifierMetadataKey = "ownerid";

        private const string CheckpointPrefixFormat = "{0}/{1}/{2}/checkpoint/";
        private const string OffsetMetadataKey = "offset";

        private const string CheckpointBlobNameFormat = "{0}/{1}/{2}/checkpoint/{3}";

        public EventHubProcessor(ILogger logger,
                                 Func<EventData, CancellationToken, Task> processEventData,
                                 int eventBatchMaximumCount,
                                 string consumerGroup,
                                 string connectionString,
                                 BlobContainerClient storageContainer,
                                 EventProcessorOptions? options = null)
            : base(eventBatchMaximumCount, consumerGroup, connectionString, options)
        {
            this.logger = logger;
            this.storageContainer = storageContainer;
            this.processEventData = processEventData;
        }

        public EventHubProcessor(ILogger logger,
                                 Func<EventData, CancellationToken, Task> processEventData,
                                 int eventBatchMaximumCount,
                                 string consumerGroup,
                                 string connectionString,
                                 string eventHubName,
                                 BlobContainerClient storageContainer,
                                 EventProcessorOptions? options = null)
            : base(eventBatchMaximumCount, consumerGroup, connectionString, eventHubName, options)
        {
            this.logger = logger;
            this.storageContainer = storageContainer;
            this.processEventData = processEventData;
        }

        public EventHubProcessor(ILogger logger,
                                 Func<EventData, CancellationToken, Task> processEventData,
                                 int eventBatchMaximumCount,
                                 string consumerGroup,
                                 string fullyQualifiedNamespace,
                                 string eventHubName,
                                 TokenCredential credential,
                                 BlobContainerClient storageContainer,
                                 EventProcessorOptions? options = null)
            : base(eventBatchMaximumCount, consumerGroup, fullyQualifiedNamespace, eventHubName, credential, options)
        {
            this.logger = logger;
            this.storageContainer = storageContainer;
            this.processEventData = processEventData;
        }

        protected override async Task<IEnumerable<EventProcessorPartitionOwnership>> ClaimOwnershipAsync(IEnumerable<EventProcessorPartitionOwnership> desiredOwnership, CancellationToken cancellationToken = default)
        {
            List<EventProcessorPartitionOwnership> claimedOwnerships = new List<EventProcessorPartitionOwnership>();

            foreach (EventProcessorPartitionOwnership ownership in desiredOwnership)
            {
                Dictionary<string, string> ownershipMetadata = new Dictionary<string, string>()
                {
                    { OwnerIdentifierMetadataKey, ownership.OwnerIdentifier },
                };

                // Construct the path to the blob and get a blob client for it so we can interact with it.
                string ownershipBlob = string.Format(OwnershipPrefixFormat + ownership.PartitionId, ownership.FullyQualifiedNamespace.ToLowerInvariant(), ownership.EventHubName.ToLowerInvariant(), ownership.ConsumerGroup.ToLowerInvariant());
                BlobClient ownershipBlobClient = storageContainer.GetBlobClient(ownershipBlob);

                try
                {
                    if (ownership.Version == null)
                    {
                        // In this case, we are trying to claim ownership of a partition which was previously unowned, and hence did not have an ownership file. To ensure only a single host grabs the partition,
                        // we use a conditional request so that we only create our blob in the case where it does not yet exist.
                        BlobRequestConditions requestConditions = new BlobRequestConditions() { IfNoneMatch = ETag.All };

                        using MemoryStream emptyStream = new MemoryStream(Array.Empty<byte>());
                        BlobContentInfo info = await ownershipBlobClient.UploadAsync(emptyStream, metadata: ownershipMetadata, conditions: requestConditions, cancellationToken: cancellationToken).ConfigureAwait(false);

                        claimedOwnerships.Add(new EventProcessorPartitionOwnership()
                        {
                            ConsumerGroup = ownership.ConsumerGroup,
                            EventHubName = ownership.EventHubName,
                            FullyQualifiedNamespace = ownership.FullyQualifiedNamespace,
                            LastModifiedTime = info.LastModified,
                            OwnerIdentifier = ownership.OwnerIdentifier,
                            PartitionId = ownership.PartitionId,
                            Version = info.ETag.ToString(),
                        });
                    }
                    else
                    {
                        // In this case, the partition is owned by some other host. The ownership file already exists, so we just need to change metadata on it. But we should only do this if the metadata has not
                        // changed between when we listed ownership and when we are trying to claim ownership, i.e. the ETag for the file has not changed.
                        BlobRequestConditions requestConditions = new BlobRequestConditions() { IfMatch = new ETag(ownership.Version) };
                        BlobInfo info = await ownershipBlobClient.SetMetadataAsync(ownershipMetadata, requestConditions, cancellationToken).ConfigureAwait(false);

                        claimedOwnerships.Add(new EventProcessorPartitionOwnership()
                        {
                            ConsumerGroup = ownership.ConsumerGroup,
                            EventHubName = ownership.EventHubName,
                            FullyQualifiedNamespace = ownership.FullyQualifiedNamespace,
                            LastModifiedTime = info.LastModified,
                            OwnerIdentifier = ownership.OwnerIdentifier,
                            PartitionId = ownership.PartitionId,
                            Version = info.ETag.ToString(),
                        });
                    }
                }
                catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobAlreadyExists || e.ErrorCode == BlobErrorCode.ConditionNotMet)
                {
                    // In this case, another host has claimed the partition before we did. That's safe to ignore. We'll still try to claim other partitions.
                }
            }

            return claimedOwnerships;
        }

        protected override async Task<IEnumerable<EventProcessorPartitionOwnership>> ListOwnershipAsync(CancellationToken cancellationToken = default)
        {
            List<EventProcessorPartitionOwnership> partitionOwnerships = new List<EventProcessorPartitionOwnership>();
            string ownershipBlobsPrefix = string.Format(OwnershipPrefixFormat, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant());

            await foreach (BlobItem blob in storageContainer.GetBlobsAsync(traits: BlobTraits.Metadata, prefix: ownershipBlobsPrefix, cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                partitionOwnerships.Add(new EventProcessorPartitionOwnership()
                {
                    ConsumerGroup = ConsumerGroup,
                    EventHubName = EventHubName,
                    FullyQualifiedNamespace = FullyQualifiedNamespace,
                    LastModifiedTime = blob.Properties.LastModified.GetValueOrDefault(),
                    OwnerIdentifier = blob.Metadata[OwnerIdentifierMetadataKey],
                    PartitionId = blob.Name[ownershipBlobsPrefix.Length..],
                    Version = blob.Properties.ETag.ToString(),
                });
            }

            return partitionOwnerships;
        }

        protected override async Task<EventProcessorCheckpoint?> GetCheckpointAsync(string partitionId, CancellationToken cancellationToken)
        {
            string checkpointName = string.Format(CheckpointBlobNameFormat, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant(), partitionId);

            try
            {
                BlobProperties properties = await storageContainer.GetBlobClient(checkpointName).GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (long.TryParse(properties.Metadata[OffsetMetadataKey], NumberStyles.Integer, CultureInfo.InvariantCulture, out long offset))
                {
                    return new EventProcessorCheckpoint()
                    {
                        ConsumerGroup = ConsumerGroup,
                        EventHubName = EventHubName,
                        FullyQualifiedNamespace = FullyQualifiedNamespace,
                        PartitionId = partitionId,
                        StartingPosition = EventPosition.FromOffset(offset, isInclusive: false),
                    };
                }
            }
            catch (RequestFailedException e) when (e.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // There's no checkpoint for this partition yet, but that's okay, so we ignore this exception.
            }

            return null;
        }

        protected async Task CheckpointAsync(TPartition partition, EventData data, CancellationToken cancellationToken = default)
        {
            string checkpointBlob = string.Format(CheckpointPrefixFormat + partition.PartitionId, FullyQualifiedNamespace.ToLowerInvariant(), EventHubName.ToLowerInvariant(), ConsumerGroup.ToLowerInvariant());
            Dictionary<string, string> checkpointMetadata = new Dictionary<string, string>()
            {
                { OffsetMetadataKey, data.Offset.ToString(CultureInfo.InvariantCulture) },
            };

            using MemoryStream emptyStream = new MemoryStream(Array.Empty<byte>());
            await storageContainer.GetBlobClient(checkpointBlob).UploadAsync(emptyStream, metadata: checkpointMetadata, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override Task OnProcessingErrorAsync(Exception exception, TPartition partition, string operationDescription, CancellationToken cancellationToken)
        {
            try
            {
                if (partition != null)
                {
                    logger.LogError("Exception on partition {partition.PartitionId} while performing {operationDescription}: {exception.Message}", partition.PartitionId, operationDescription, exception.Message);
                }
                else
                {
                   logger.LogError("Exception while performing {operationDescription}: {exception.Message}", operationDescription, exception.Message);
                }
            }
            catch
            {
                // Catch and ignore, we should not allow exceptions to bubble out of this method.
            }

            return Task.CompletedTask;
        }

        protected override async Task OnProcessingEventBatchAsync(IEnumerable<EventData> events, TPartition partition, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Received batch of {numOfEvents} events for partition {partitionId}", events.Count(), partition.PartitionId);

                var tasks = new List<Task>();
                foreach (var telemetryEvent in events)
                {
                    tasks.Add(processEventData(telemetryEvent, cancellationToken));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (events.Any())
                {
                    await CheckpointAsync(partition, events.Last(), cancellationToken).ConfigureAwait(false);
                }
            }
            catch
            {
                // Catch and ignore, we should not allow exceptions to bubble out of this method.
            }
        }
    }
}
