﻿// -----------------------------------------------------------------------
// <copyright file="AzureDigitalTwinsGraphManager.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.AzureDigitalTwins
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Threading;
    using global::Azure;
    using global::Azure.Core.Pipeline;
    using global::Azure.DigitalTwins.Core;
    using global::Azure.Identity;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    /// <summary>
    /// Output graph manager supporting writing to Azure Digital Twins.
    /// </summary>
    /// <typeparam name="TOptions">Ingestion manager options type.</typeparam>
    public class AzureDigitalTwinsGraphManager<TOptions> : IOutputGraphManager
        where TOptions : IngestionManagerOptions
    {
        private readonly ConcurrentQueue<DigitalTwinsClient> queue = new ConcurrentQueue<DigitalTwinsClient>();
        private readonly ParallelOptions parallelOptions;
        private IngestionManagerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDigitalTwinsGraphManager{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">Local logger.</param>
        /// <param name="options">Ingestion manager options.</param>
        /// <param name="twinMappingIndexer">Twin mapping index cache.</param>
        /// <param name="telemetryClient">Application Insights telemetry client for remote metrics tracking.</param>
        /// <param name="httpClientFactory">Generator of HttpClients with custom additions.</param>
        /// <param name="skipUpload">Option denoting whether the manager will upload twins to target
        /// Azure Digital Twins environment. If skipUpload is <c>true</c>, only the cache will be updated.</param>
        public AzureDigitalTwinsGraphManager(ILogger<AzureDigitalTwinsGraphManager<TOptions>> logger,
                               IOptions<TOptions> options,
                               ITwinMappingIndexer twinMappingIndexer,
                               TelemetryClient telemetryClient,
                               IHttpClientFactory httpClientFactory,
                               bool skipUpload = false)
        {
            Logger = logger;
            this.options = options.Value;
            TelemetryClient = telemetryClient;
            TwinMappingIndexer = twinMappingIndexer;
            SkipUpload = skipUpload;

            for (int i = 0; i < this.options.MaxDegreeOfParallelism; i++)
            {
                queue.Enqueue(new DigitalTwinsClient(new Uri(options.Value.AzureDigitalTwinsEndpoint), new DefaultAzureCredential(), new DigitalTwinsClientOptions
                {
                    Transport = new HttpClientTransport(httpClientFactory.CreateClient("Microsoft.SmartPlaces.Facilities")),
                }));
            }

            parallelOptions = new ()
            {
                MaxDegreeOfParallelism = options.Value.MaxDegreeOfParallelism,
            };
        }

        /// <summary>
        /// Gets local logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets App Insights telemetry client.
        /// </summary>
        protected TelemetryClient TelemetryClient { get; }

        /// <summary>
        /// Gets twin mapping index cache.
        /// </summary>
        protected ITwinMappingIndexer TwinMappingIndexer { get; }

        /// <summary>
        /// Gets a value indicating whether to upload graph to Azure Digital Twins.
        /// Otherwise, only the cache will be updated.
        /// </summary>
        protected bool SkipUpload { get; }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetModelAsync(CancellationToken cancellationToken)
        {
            // Get all of the Models to be used for creating instances of twins
            if (queue.TryDequeue(out var digitalTwinsClient))
            {
                var modelsResults = digitalTwinsClient.GetModelsAsync(new GetModelsOptions { IncludeModelDefinition = true }, cancellationToken);
                var modelList = new List<string>();
                queue.Enqueue(digitalTwinsClient);

                await foreach (DigitalTwinsModelData md in modelsResults)
                {
                    // TODO: jobee - Add error handling for models which do not have a DtdlModel (is this even possible?)
                    if (md.DtdlModel != null)
                    {
                        modelList.Add(md.DtdlModel);
                    }
                }

                return modelList;
            }
            else
            {
                throw new Exception("Unable to get ADT Client instance");
            }
        }

        /// <inheritdoc/>
        public virtual async Task UploadGraphAsync(Dictionary<string, BasicDigitalTwin> twins, Dictionary<string, BasicRelationship> relationships, CancellationToken cancellationToken)
        {
            if (!SkipUpload)
            {
                await ImportTwinsAsync(twins);
                await ImportRelationshipsAsync(relationships);
            }
            else
            {
                Logger.LogInformation("Skip upload is set to true. Skipped loading ADT Instance.");
            }

            await UpdateCache(twins);
        }

        /// <summary>
        /// Add a set of twins to the mapping index cache.
        /// </summary>
        /// <param name="twins">The twins that will be cached. Map key is the dtId.</param>
        /// <returns>An awaitable task.</returns>
        protected async Task UpdateCache(Dictionary<string, BasicDigitalTwin> twins)
        {
            Logger.LogInformation("Upserting mapping of {count} twins to Cache", twins.Count);
            var cacheTasks = new List<Task>();
            foreach (var twin in twins)
            {
                if (twin.Value.Contents.TryGetValue("externalIds", out var externalIds))
                {
                    if (externalIds is IDictionary<string, string> externalIdDictionary)
                    {
                        if (externalIdDictionary.TryGetValue("mappingKey", out var mappingKey))
                        {
                            var cacheEntry = new TwinMapEntry
                            {
                                TargetTwinId = twin.Key,
                                TargetModelId = twin.Value.Metadata.ModelId,
                            };

                            cacheTasks.Add(TwinMappingIndexer.UpsertTwinIndexAsync(mappingKey, cacheEntry));

                            if (cacheTasks.Count > 100)
                            {
                                await CommitCache(cacheTasks);
                            }
                        }
                    }
                }
            }

            Logger.LogInformation("Starting last cache commit.");
            await CommitCache(cacheTasks);
        }

        /// <summary>
        /// Await a set of caching tasks; effectively, ensure all those batched cache writes have been completed.
        /// </summary>
        /// <param name="cacheTasks">Caching tasks to commit.</param>
        /// <returns>An awaitable task.</returns>
        private async Task CommitCache(List<Task> cacheTasks)
        {
            Logger.LogInformation("Starting cache commit.");

            await Parallel.ForEachAsync(cacheTasks,
                                        parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount / 2, 1) },
                                        async (cacheTask, cancellationToken) =>
                                        {
                                            try
                                            {
                                                await cacheTask;
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.LogError("Failed to upsert mapping: {exception}", ex.Message);
                                            }
                                        });
            cacheTasks.Clear();

            Logger.LogInformation("Completing cache commit.");
        }

        /// <summary>
        /// Asynchronously import a set of relationships into the target Azure Digital Twins graph.
        /// </summary>
        /// <param name="relationships">Relationships to upload. Map key is the relationship ID.</param>
        /// <param name="retryAttempt">Retry counter.</param>
        /// <returns>An awaitable task.</returns>
        protected virtual async Task ImportRelationshipsAsync(IDictionary<string, BasicRelationship> relationships, int retryAttempt = 0)
        {
            // Make sure we don't get caught in an infinite retry loop
            if (retryAttempt >= options.MaxRetryAttempts)
            {
                return;
            }

            // Upload the relationships to ADT
            Logger.LogInformation("Total Relationships to create: {relationshipsCount}", relationships.Count);
            var relationshipCreateSuccesses = 0;
            var relationshipCreateFailures = 0;
            var relationshipUpdateSuccesses = 0;
            var relationshipUpdateFailures = 0;
            var relationshipUpdateSkips = 0;
            var failedRelationships = new ConcurrentDictionary<string, BasicRelationship>();

            // We need to shuffle this list to try to reduce the chances of going over the limit for the number of updates per twin per second
            var relationshipList = new List<BasicRelationship>(relationships.Values).Shuffle();

            var relationshipMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "Relationships", Metrics.ActionDimensionName, Metrics.RelationshipTypeDimensionName, Metrics.StatusDimensionName);

            await Parallel.ForEachAsync(relationshipList, parallelOptions, async (relationship, cancellationToken) =>
            {
                if (queue.TryDequeue(out var digitalTwinsClient))
                {
                    try
                    {
                        BasicRelationship? existingRelationship = null;
                        try
                        {
                            var existingRelationshipResponse = await digitalTwinsClient.GetRelationshipAsync<BasicRelationship>(relationship.SourceId, relationship.Id, cancellationToken);
                            existingRelationship = existingRelationshipResponse.Value;
                        }
                        catch (RequestFailedException ex) when (ex.ErrorCode == "RelationshipNotFound")
                        {
                        }

                        if (existingRelationship == null)
                        {
                            try
                            {
                                await digitalTwinsClient.CreateOrReplaceRelationshipAsync(relationship.SourceId, relationship.Id, relationship, cancellationToken: cancellationToken);
                                TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, relationship.Name, Metrics.SucceededStatusDimension);
                                relationshipCreateSuccesses++;
                            }
                            catch (RequestFailedException ex)
                            {
                                // If we are over the limit, sleep for a bit then add the relationship to the retry list
                                if (ex.Status == 429 || ex.Status == 503)
                                {
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, relationship.Name, Metrics.ThrottledStatusDimension);
                                    Logger.LogError("Connection to ADT Throttled while adding relationship. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationToken);
                                    failedRelationships.TryAdd(relationship.Id, relationship);
                                }
                                else
                                {
                                    relationshipCreateFailures++;

                                    Logger.LogError(ex, "Failed to add relationship: {relationship}", JsonSerializer.Serialize(relationship));
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, relationship.Name, Metrics.FailedStatusDimension);
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                var jsonPatchDocumentHasUpdates = TwinMergeHelper.TryCreatePatchDocument(existingRelationship, relationship, out var jsonPatchDocument);

                                if (jsonPatchDocumentHasUpdates)
                                {
                                    await digitalTwinsClient.UpdateRelationshipAsync(relationship.SourceId, relationship.Id, jsonPatchDocument, cancellationToken: cancellationToken);
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, relationship.Name, Metrics.SucceededStatusDimension);
                                    relationshipUpdateSuccesses++;
                                }
                                else
                                {
                                    relationshipUpdateSkips++;
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, relationship.Name, Metrics.SkippedStatusDimension);
                                }
                            }
                            catch (RequestFailedException ex)
                            {
                                // If we are over the limit, sleep for a bit then add the relationship to the retry list
                                if (ex.Status == 429 || ex.Status == 503)
                                {
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, relationship.Name, Metrics.ThrottledStatusDimension);
                                    Logger.LogError("Connection to ADT Throttled while updating relationship. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationToken);
                                    failedRelationships.TryAdd(relationship.Id, relationship);
                                }
                                else
                                {
                                    relationshipUpdateFailures++;

                                    Logger.LogError(ex, "Failed to update relationship: {relationship}", JsonSerializer.Serialize(relationship));
                                    TelemetryClient.GetMetric(relationshipMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, relationship.Name, Metrics.FailedStatusDimension);
                                }
                            }
                        }
                    }
                    catch (RequestFailedException ex) when (ex.ErrorCode == "DigitalTwinNotFound")
                    {
                        relationshipCreateFailures++;
                        Logger.LogError(ex, "Source Twin Not Found. Cannot create or update relationship: {relationship}", JsonSerializer.Serialize(relationship));
                    }
                    finally
                    {
                        queue.Enqueue(digitalTwinsClient);
                    }
                }
                else
                {
                    Logger.LogError("Failed to get instance of ADT Client.");
                    await Task.Delay(options.RetryDelayInMs, cancellationToken);
                    failedRelationships.TryAdd(relationship.Id, relationship);
                }

                // Output a status every 1000 relationships
                if ((relationshipCreateSuccesses + relationshipCreateFailures + relationshipUpdateSuccesses + relationshipUpdateFailures + relationshipUpdateSkips) % 1000 == 0)
                {
                    Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}, Successful relationship updates: {relationshipUpdateSuccesses}, Failed Relationship updates: {relationshipUpdateFailures}, Skipped Relationship updates: {relationshipUpdateSkips}",
                                          DateTimeOffset.Now,
                                          relationships.Count,
                                          relationshipCreateSuccesses,
                                          relationshipCreateFailures,
                                          relationshipUpdateSuccesses,
                                          relationshipUpdateFailures,
                                          relationshipUpdateSkips);
                }
            });

            Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}, Successful relationship updates: {relationshipUpdateSuccesses}, Failed Relationship updates: {relationshipUpdateFailures}, Skipped Relationship updates: {relationshipUpdateSkips}",
                                  DateTimeOffset.Now,
                                  relationships.Count,
                                  relationshipCreateSuccesses,
                                  relationshipCreateFailures,
                                  relationshipUpdateSuccesses,
                                  relationshipUpdateFailures,
                                  relationshipUpdateSkips);

            if (!failedRelationships.IsEmpty)
            {
                Logger.LogInformation("{timestamp}: Sending {failedRelationshipsCount} relationships to be retried.", DateTimeOffset.Now, failedRelationships.Count);
                await ImportRelationshipsAsync(failedRelationships, ++retryAttempt);
            }
        }

        /// <summary>
        /// Asynchronously import a set of digital twins into the target Azure Digital Twins graph.
        /// </summary>
        /// <param name="twins">Twins to upload. Map key is the dtId.</param>
        /// <param name="retryAttempt">Retry counter.</param>
        /// <returns>An awaitable task.</returns>
        protected virtual async Task ImportTwinsAsync(IDictionary<string, BasicDigitalTwin> twins, int retryAttempt = 0)
        {
            // Make sure we don't get caught in an infinite retry loop
            if (retryAttempt >= options.MaxRetryAttempts)
            {
                return;
            }

            // Upload the twins to ADT
            Logger.LogInformation("Total Twins to create: {twinsCount}", twins.Count);
            var twinCreateSuccesses = 0;
            var twinCreateFailures = 0;
            var twinUpdateSuccesses = 0;
            var twinUpdateFailures = 0;
            var twinUpdateSkips = 0;

            var failedTwins = new ConcurrentDictionary<string, BasicDigitalTwin>();
            var twinMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "Twins", Metrics.ActionDimensionName, Metrics.ModelIdDimensionName, Metrics.StatusDimensionName);

            await Parallel.ForEachAsync(twins, parallelOptions, async (twin, cancellationtoken) =>
            {
                if (queue.TryDequeue(out var digitalTwinsClient))
                {
                    try
                    {
                        BasicDigitalTwin? existingTwin = null;

                        try
                        {
                            // First, try to get the twin
                            existingTwin = await digitalTwinsClient.GetDigitalTwinAsync<BasicDigitalTwin>(twin.Value.Id, cancellationToken: cancellationtoken);
                        }
                        catch (RequestFailedException ex) when (ex.ErrorCode == "DigitalTwinNotFound")
                        {
                        }

                        if (existingTwin == null)
                        {
                            try
                            {
                                await digitalTwinsClient.CreateOrReplaceDigitalTwinAsync(twin.Value.Id, twin.Value, cancellationToken: cancellationtoken);
                                TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, twin.Value.Metadata.ModelId, Metrics.SucceededStatusDimension);
                                twinCreateSuccesses++;
                            }
                            catch (RequestFailedException ex)
                            {
                                // If we are over the limit, sleep for a bit then add the twin to the retry list
                                if (ex.Status == 429 || ex.Status == 503)
                                {
                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, twin.Value.Metadata.ModelId, Metrics.ThrottledStatusDimension);
                                    Logger.LogError("Connection to ADT Throttled while adding twin. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationtoken);
                                    failedTwins.TryAdd(twin.Key, twin.Value);
                                }
                                else
                                {
                                    twinCreateFailures++;

                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.CreateActionDimension, twin.Value.Metadata.ModelId, Metrics.FailedStatusDimension);
                                    Logger.LogError(ex, "Failed to add twin: {twin}", JsonSerializer.Serialize(twin));
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                var jsonPatchDocumentHasUpdates = TwinMergeHelper.TryCreatePatchDocument(existingTwin, twin.Value, out var jsonPatchDocument);

                                if (jsonPatchDocumentHasUpdates)
                                {
                                    await digitalTwinsClient.UpdateDigitalTwinAsync(twin.Value.Id, jsonPatchDocument, cancellationToken: cancellationtoken);
                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, twin.Value.Metadata.ModelId, Metrics.SucceededStatusDimension);
                                    twinUpdateSuccesses++;
                                }
                                else
                                {
                                    twinUpdateSkips++;
                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, twin.Value.Metadata.ModelId, Metrics.SkippedStatusDimension);
                                }
                            }
                            catch (RequestFailedException ex)
                            {
                                // If we are over the limit or service is unavailable, sleep for a bit then add the twin to the retry list
                                if (ex.Status == 429 || ex.Status == 503)
                                {
                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, twin.Value.Metadata.ModelId, Metrics.ThrottledStatusDimension);
                                    Logger.LogError("Connection to ADT Throttled while update twin. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationtoken);
                                    failedTwins.TryAdd(twin.Key, twin.Value);
                                }
                                else
                                {
                                    twinUpdateFailures++;

                                    TelemetryClient.GetMetric(twinMetricIdentifier).TrackValue(1, Metrics.UpdateActionDimension, twin.Value.Metadata.ModelId, Metrics.FailedStatusDimension);
                                    Logger.LogError(ex, "Failed to update twin: {twin}", JsonSerializer.Serialize(twin));
                                }
                            }
                        }
                    }
                    finally
                    {
                        queue.Enqueue(digitalTwinsClient);
                    }
                }
                else
                {
                    Logger.LogError("Failed to get instance of ADT Client.");
                    await Task.Delay(options.RetryDelayInMs, cancellationtoken);
                    failedTwins.TryAdd(twin.Key, twin.Value);
                }

                // Output a status every 1000 relationships
                if ((twinCreateSuccesses + twinCreateFailures + twinUpdateSuccesses + twinUpdateFailures + twinUpdateSkips) % 1000 == 0)
                {
                    Logger.LogInformation("{timestamp}: Total Twins: {twinCount}, Successful twin creates: {twinCreateSuccesses}, Failed twin creates: {twinCreateFailures}, Successful twin updates: {twinUpdateSuccesses}, Failed twin updates: {twinUpdateFailures}, Skipped twin updates: {twinUpdateSkips}",
                                          DateTimeOffset.Now,
                                          twins.Count,
                                          twinCreateSuccesses,
                                          twinCreateFailures,
                                          twinUpdateSuccesses,
                                          twinUpdateFailures,
                                          twinUpdateSkips);
                }
            });

            Logger.LogInformation("{timestamp}: Total Twins: {twinCount}, Successful twin creates: {twinCreateSuccesses}, Failed twin creates: {twinCreateFailures}, Successful twin updates: {twinUpdateSuccesses}, Failed twin updates: {twinUpdateFailures}, Skipped twin updates: {twinUpdateSkips}",
                                  DateTimeOffset.Now,
                                  twins.Count,
                                  twinCreateSuccesses,
                                  twinCreateFailures,
                                  twinUpdateSuccesses,
                                  twinUpdateFailures,
                                  twinUpdateSkips);

            if (!failedTwins.IsEmpty)
            {
                Logger.LogInformation("{timestamp}:Sending {twinCreateFailures} twins to be retried.", DateTimeOffset.Now, twinCreateFailures);
                await ImportTwinsAsync(failedTwins, ++retryAttempt);
            }
        }
    }
}
