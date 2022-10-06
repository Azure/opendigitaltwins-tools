//-----------------------------------------------------------------------
// <copyright file="AdtGraphManager.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IngestionManager.AzureDigitalTwins
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using Azure;
    using Azure.DigitalTwins.Core;
    using Azure.Identity;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using IngestionManager.Extensions;
    using IngestionManager.Interfaces;

    public class AdtGraphManager<TOptions> : IOutputGraphManager
        where TOptions : IngestionManagerOptions
    {
        private readonly ConcurrentQueue<DigitalTwinsClient> queue = new ConcurrentQueue<DigitalTwinsClient>();
        private readonly ParallelOptions parallelOptions;
        private IngestionManagerOptions options;

        public AdtGraphManager(ILogger<AdtGraphManager<TOptions>> logger,
                               IOptions<TOptions> options,
                               ITwinMappingIndexer twinMappingIndexer,
                               TelemetryClient telemetryClient,
                               bool skipUpload = false)
        {
            Logger = logger;
            this.options = options.Value;
            TelemetryClient = telemetryClient;
            TwinMappingIndexer = twinMappingIndexer;
            SkipUpload = skipUpload;

            for (int i = 0; i < this.options.MaxDegreeOfParallelism; i++)
            {
                queue.Enqueue(new DigitalTwinsClient(new Uri(options.Value.AzureDigitalTwinsEndpoint), new DefaultAzureCredential()));
            }

            parallelOptions = new ()
            {
                MaxDegreeOfParallelism = options.Value.MaxDegreeOfParallelism,
            };
        }

        protected ILogger Logger { get; }

        protected TelemetryClient TelemetryClient { get; }

        protected ITwinMappingIndexer TwinMappingIndexer { get; }

        protected bool SkipUpload { get; }

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
                            cacheTasks.Add(TwinMappingIndexer.UpsertTwinIndexAsync(mappingKey, twin.Key));

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
            var failedRelationships = new ConcurrentDictionary<string, BasicRelationship>();

            // We need to shuffle this list to try to reduce the chances of going over the limit for the number of updates per twin per second
            var relationshipList = new List<BasicRelationship>(relationships.Values).Shuffle();
            var relationshipsMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceRelationshipAsync", Metrics.RelationshipTypeDimensionName);
            var throttledMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceRelationshipAsyncThrottle");
            var failedMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceRelationshipAsyncFailed", Metrics.RelationshipTypeDimensionName);

            await Parallel.ForEachAsync(relationshipList, parallelOptions, async (relationship, cancellationToken) =>
            {
                if (queue.TryDequeue(out var digitalTwinsClient))
                {
                    try
                    {
                        await digitalTwinsClient.CreateOrReplaceRelationshipAsync(relationship.SourceId, relationship.Id, relationship, cancellationToken: cancellationToken);
                        TelemetryClient.GetMetric(relationshipsMetricIdentifier).TrackValue(1, relationship.Name);
                        relationshipCreateSuccesses++;
                    }
                    catch (RequestFailedException ex)
                    {
                        relationshipCreateFailures++;

                        // If we are over the limit, sleep for a bit then add the relationship to the retry list
                        // Todo: jobee - are there any other cases that need to be retried?
                        if (ex.Status == 429)
                        {
                            TelemetryClient.GetMetric(throttledMetricIdentifier).TrackValue(1);
                            Logger.LogError("Connection to ADT Throttled while adding relationship. Sleeping.");
                            await Task.Delay(options.RetryDelayInMs, cancellationToken);
                            failedRelationships.TryAdd(relationship.Id, relationship);
                        }
                        else
                        {
                            Logger.LogError(ex, "Failed to add relationship: {relationships}", JsonSerializer.Serialize(relationship));
                            TelemetryClient.GetMetric(failedMetricIdentifier).TrackValue(1, relationship.Name);
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
                    await Task.Delay(options.RetryDelayInMs, cancellationToken);
                    failedRelationships.TryAdd(relationship.Id, relationship);
                }

                // Output a status every 1000 relationships
                if ((relationshipCreateSuccesses + relationshipCreateFailures) % 1000 == 0)
                {
                    Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}",
                                          DateTimeOffset.Now,
                                          relationships.Count,
                                          relationshipCreateSuccesses,
                                          relationshipCreateFailures);
                }
            });

            Logger.LogInformation("{timestamp}: Total Relationships: {twinCount}, Successful relationship creates: {relationshipCreateSuccesses}, Failed Relationship creates: {relationshipCreateFailures}",
                                  DateTimeOffset.Now,
                                  relationships.Count,
                                  relationshipCreateSuccesses,
                                  relationshipCreateFailures);

            if (!failedRelationships.IsEmpty)
            {
                Logger.LogInformation("{timestamp}: Sending {failedRelationshipsCount} relationships to be retried.", DateTimeOffset.Now, failedRelationships.Count);
                await ImportRelationshipsAsync(failedRelationships, ++retryAttempt);
            }
        }

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
            var createTwinMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceDigitalTwinAsync", Metrics.ModelIdDimensionName);
            var createThrottledMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceDigitalTwinAsyncThrottled");
            var createFailedMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "CreateOrReplaceDigitalTwinAsyncFailed", Metrics.TwinDimensionName);
            var updateTwinMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "UpdateDigitalTwinAsync", Metrics.ModelIdDimensionName);
            var updateThrottledMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "UpdateDigitalTwinAsyncThrottled");
            var updateFailedMetricIdentifier = new MetricIdentifier(Metrics.DefaultNamespace, "UpdateDigitalTwinAsyncFailed", Metrics.TwinDimensionName);

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
                                TelemetryClient.GetMetric(createTwinMetricIdentifier).TrackValue(1, twin.Value.Metadata.ModelId);
                                twinCreateSuccesses++;
                            }
                            catch (RequestFailedException ex)
                            {
                                twinCreateFailures++;

                                // If we are over the limit, sleep for a bit then add the twin to the retry list
                                if (ex.Status == 429)
                                {
                                    TelemetryClient.GetMetric(createThrottledMetricIdentifier).TrackValue(1);
                                    Logger.LogError("Connection to ADT Throttled while adding twin. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationtoken);
                                    failedTwins.TryAdd(twin.Key, twin.Value);
                                }
                                else
                                {
                                    TelemetryClient.GetMetric(createFailedMetricIdentifier).TrackValue(1, twin.Value.Metadata.ModelId);
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
                                    TelemetryClient.GetMetric(updateTwinMetricIdentifier).TrackValue(1, twin.Value.Metadata.ModelId);
                                    twinUpdateSuccesses++;
                                }
                                else
                                {
                                    twinUpdateSkips++;
                                }
                            }
                            catch (RequestFailedException ex)
                            {
                                twinUpdateFailures++;

                                // If we are over the limit, sleep for a bit then add the twin to the retry list
                                if (ex.Status == 429)
                                {
                                    TelemetryClient.GetMetric(updateThrottledMetricIdentifier).TrackValue(1);
                                    Logger.LogError("Connection to ADT Throttled while update twin. Sleeping.");
                                    await Task.Delay(options.RetryDelayInMs, cancellationtoken);
                                    failedTwins.TryAdd(twin.Key, twin.Value);
                                }
                                else
                                {
                                    TelemetryClient.GetMetric(updateFailedMetricIdentifier).TrackValue(1, twin.Value.Metadata.ModelId);
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
