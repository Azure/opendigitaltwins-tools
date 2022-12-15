//-----------------------------------------------------------------------
// <copyright file="TelemetryIngestionProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Telemetry.Processors
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Core.Pipeline;
    using Azure.DigitalTwins.Core;
    using Azure.Identity;
    using Azure.Messaging.EventHubs;
    using Google.Protobuf;
    using Mapped.Cloud.Types;
    using Mapped.Gateway;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Azure.DigitalTwins.Parser;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Telemetry.Exceptions;
    using Telemetry.Interfaces;

    public class TelemetryIngestionProcessor<TOptions> : ITelemetryIngestionProcessor
        where TOptions : IngestionManagerOptions
    {
        private readonly DigitalTwinsClient digitalTwinsClient;
        private readonly ILogger<TelemetryIngestionProcessor<TOptions>> logger;
        private readonly TelemetryClient telemetryClient;
        private readonly ITwinMappingIndexer twinMappingIndexer;
        private readonly TOptions options;

        // Target Telemetry Values
        private const string telemetryValueRoot = "lastKnownValue";
        private const string telemetryValueKey = "value";
        private const string telemetryTimestampKey = "timestamp";

        private static IDictionary<string, DTEntityKind> modelIdToTypeCache = new ConcurrentDictionary<string, DTEntityKind>();

        //Metrics
        private static readonly string metricsNamespace = "Telemetry";
        private readonly MetricIdentifier twinUpdateMetric = new MetricIdentifier(metricsNamespace, "TwinUpdate","Status","Reason");
        private readonly MetricIdentifier telemetryTypeMetric = new MetricIdentifier(metricsNamespace, "TelemetryType", "SourceType", "TargetType");

        /// <summary>
        /// Setup for how to process an event flowing across an EventHub.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="twinMappingIndexer"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="telemetryClient"></param>
        public TelemetryIngestionProcessor(ILogger<TelemetryIngestionProcessor<TOptions>> logger,
                                           IOptions<TOptions> options,
                                           ITwinMappingIndexer twinMappingIndexer,
                                           IHttpClientFactory httpClientFactory,
                                           TelemetryClient telemetryClient)
        {
            this.logger = logger;
            this.options = options.Value;
            this.twinMappingIndexer = twinMappingIndexer;
            this.telemetryClient = telemetryClient;

            var httpClient = httpClientFactory.CreateClient("Microsoft.SmartPlaces.Facilities");

            logger.LogInformation("TwinMappingIndexer: {type}", twinMappingIndexer.GetType().Name);
            digitalTwinsClient = new DigitalTwinsClient(new Uri(this.options.AzureDigitalTwinsEndpoint), new DefaultAzureCredential(), new DigitalTwinsClientOptions
            {
                Transport = new HttpClientTransport(httpClient),
            });
        }

        /// <summary>
        /// This method defines what actions should be taken to translate from source telemetry data and ingest
        /// into Azure Digital Twins.
        /// </summary>
        /// <param name="telemetryEvent">Contains the values passed through the IotHub into the EventHub to be processed by Telemetry.</param>
        /// <param name="cancellationToken">A way to stop things.</param>
        public async Task IngestFromEventHubAsync(EventData telemetryEvent, CancellationToken cancellationToken)
        {
            string status = "Failed";
            string reason = "Unknown";
            try
            {
                // Determine where to send the telemetryEventData
                var twinIdLookupCache = await GetTwinMap(telemetryEvent);
                if (twinIdLookupCache.twinMap is not null)
                {
                    RedisPoint point;
                    try
                    {
                        //Decode the event data
                        point = RedisPoint.Parser.ParseFrom(telemetryEvent.EventBody);
                        logger.LogTrace("MappedProtobuf: {protobuf} ParsedValue: {originalValue}", BitConverter.ToString(telemetryEvent.EventBody.ToArray()), point);
                    }
                    catch (InvalidProtocolBufferException ex)
                    {
                        logger.LogError(ex, "Unable to parse data from Mapped");
                        reason = "InvalidMessageBody";
                        return;
                    }

                    var retryTypeAttempts = 1;
                    do
                    {
                        //Determine if we know the target type of this model, otherwise look it up in AzureDigitalTwins
                        if (!modelIdToTypeCache.TryGetValue(twinIdLookupCache.twinMap.TargetModelId, out DTEntityKind targetType))
                        {
                            // This sample is using a consistent field `lastKnownValue.value` across all DTDL Models to store Telemetry
                            try
                            {
                                targetType = await ModelProcessor.GetEntityKindFromModelIdAsync(digitalTwinsClient, twinIdLookupCache.twinMap.TargetModelId, $"contents:__{telemetryValueRoot}:_schema:_fields:__{telemetryValueKey}", cancellationToken);
                            }
                            catch
                            {
                                logger.LogError("Telemetry EntityKind lookup failed for $dtId: {adtTwinId}", twinIdLookupCache.twinMap.TargetTwinId);
                                throw;
                            }

                            try
                            {
                                modelIdToTypeCache.Add(twinIdLookupCache.twinMap.TargetModelId, targetType);
                            }
                            catch (ArgumentException ex) when (ex.Message == "The key already existed in the dictionary.")
                            {
                                targetType = modelIdToTypeCache[twinIdLookupCache.twinMap.TargetModelId];
                            }
                        }

                        try
                        {
                            var updateTwinData = CreatePatch(point, targetType);

                            logger.LogDebug("$dtId: {adtTwinId} TwinPatchBody: {body}", twinIdLookupCache.twinMap.TargetTwinId, updateTwinData.ToString());

                            var response = await digitalTwinsClient.UpdateDigitalTwinAsync(twinIdLookupCache.twinMap.TargetTwinId, updateTwinData, cancellationToken: cancellationToken);

                            if (response.IsError)
                            {
                                reason = response.ReasonPhrase;
                            }
                            else
                            {
                                status = "Succeeded";
                                reason = "Updated";
                                break;
                            }
                        }
                        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.BadRequest && ex.ErrorCode == "ValidationFailed" && ex.Message.StartsWith("Expected value of type"))
                        {
                            // Data previously cached for targetType does not match current AzureDigitalTwins DTDL Validations.
                            modelIdToTypeCache.Remove(twinIdLookupCache.twinMap.TargetModelId);
                            logger.LogWarning("Failed to replace, refreshing local cache of targetDataType. ErrorMessage: {ErrorMessage}", ex.Message);
                            reason = "ADTValidationError";
                        }
                        catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.BadRequest && ex.ErrorCode == "JsonPatchInvalid" && ex.Message.StartsWith("Could not resolve path"))
                        {
                            /*
                                This sample is saving telemetry to a complex object. To avoid deleting data the update path does not
                                include the root of the complex object. AzureDigitalTwins uses a JsonPatchDocument which cannot find the path to fields
                                within the object if the root has not been initialized. This retry initializes the root level objects.
                            */
                            logger.LogWarning("Failed to replace, attempting to add. ErrorMessage: {ErrorMessage}", ex.Message);

                            var addTwinData = CreatePatch(point, targetType, true);

                            logger.LogDebug("$dtId: {adtTwinId} TwinPatchBody: {body}", twinIdLookupCache.twinMap.TargetTwinId, addTwinData.ToString());

                            var response = await digitalTwinsClient.UpdateDigitalTwinAsync(twinIdLookupCache.twinMap.TargetTwinId, addTwinData, cancellationToken: cancellationToken);

                            if (response.IsError)
                            {
                                reason = response.ReasonPhrase;
                            }
                            else
                            {
                                status = "Succeeded";
                                reason = "Added";
                                break;
                            }
                        }
                        catch (InvalidCastException ex)
                        {
                            logger.LogError(ex, $"Error processing twin Id: {twinIdLookupCache.twinMap.TargetTwinId}.");
                            throw;
                        }
                    } while (--retryTypeAttempts>-1);
                }
                else
                {
                    status = "Dropped";
                    reason = twinIdLookupCache.failureReason;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to ingest telemetry into AzureDigitalTwins");
                reason = $"{e.GetType()}";
            }
            finally
            {
                telemetryClient.GetMetric(twinUpdateMetric).TrackValue(1, status, string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason);
            }
        }

        /// <summary>
        /// Reaches out to the cloud redis which has been populated by the Topology project to determine 
        /// details about the incoming telemetry data such as TwinId and ModelId for the raw data.
        /// </summary>
        /// <param name="telemetryEvent">Raw event off of the eventHub.</param>
        /// <returns>The data stored on the cloud redis by the Topology project.</returns>
        private async Task<(TwinMapEntry? twinMap, string failureReason)> GetTwinMap(EventData telemetryEvent)
        {
            TwinMapEntry? twinMap = null;
            string failureReason = "Unknown";
            if (telemetryEvent.Properties.TryGetValue("mappingKey", out var mappingKey))
            {
                twinMap = await twinMappingIndexer.GetTwinIndexAsync((string)mappingKey).ConfigureAwait(false);
                if (twinMap is null)
                {
                    logger.LogError("MappingKey not found in twin lookup cache: {mappingKey}", mappingKey);
                    failureReason = "MappingKeyNotFoundInTwinIdLookupCache";
                }
            }
            else
            {
                logger.LogError("MappingKey not found on telemetryEvent");
                failureReason = "MappingKeyNotFoundInEventHubProperties";
            }

            return (twinMap, failureReason);
        }

        /// <summary>
        /// Generates a JsonPatchDocument conforming to the desired data model.
        /// </summary>
        /// <param name="point">Incoming raw telemetry data.</param>
        /// <param name="targetType">What the DTDL Model says the datatype should be.</param>
        /// <param name="isAdd">Whether to generate an Add or Replace JsonPatchDocument.</param>
        /// <returns>A properly formatted JsonPatchDocument which AzureDigitalTwins should accept.</returns>
        private JsonPatchDocument CreatePatch(RedisPoint point, DTEntityKind targetType, bool isAdd = false)
        {
            var patch = new JsonPatchDocument();
            dynamic? value = null;
            try
            {
                value = TranslateType(point.PresentValue, targetType);
                if (isAdd)
                {
                    patch.AppendAdd<object>($"/{telemetryValueRoot}", new());
                    patch.AppendAdd($"/{telemetryValueRoot}/{telemetryValueKey}", value);
                    patch.AppendAdd($"/{telemetryValueRoot}/{telemetryTimestampKey}", point.LastUpdate.ToDateTime());
                }
                else
                {
                    // Counting here since the add functionality is a fallback and would duplicate data
                    telemetryClient.GetMetric(telemetryTypeMetric).TrackValue(1, point.PresentValue.ValueCase.ToString(), targetType.ToString());

                    patch.AppendReplace($"/{telemetryValueRoot}/{telemetryValueKey}", value);
                    patch.AppendReplace($"/{telemetryValueRoot}/{telemetryTimestampKey}", point.LastUpdate.ToDateTime());
                }
            }
            catch (ArgumentException ex)
            {
                throw new TelemetryValueException($"Failed to create patch document with value '{value}' when converting {point.PresentValue.ValueCase} to {targetType}", ex);
            }

            return patch;
        }

        /// <summary>
        /// Transforms and formats data between Mapped TypeValues and DTDL DTEntityKind.
        /// </summary>
        /// <param name="sourceData">Incoming telemetry data.</param>
        /// <param name="targetType">Desired outgoing data.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">When a conversion has been attempted in code, but the data is not parsable to the desired datatype.</exception>
        /// <exception cref="NotImplementedException">When a conversion has not been determined in code.</exception>
        private dynamic TranslateType(TypedValue sourceData, DTEntityKind targetType)
        {
            switch (sourceData.ValueCase)
            {
                case TypedValue.ValueOneofCase.BoolArrayValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.BoolArrayValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.BoolValue:
                    switch (targetType)
                    {
                        case DTEntityKind.Boolean:
                            return sourceData.BoolValue;
                        case DTEntityKind.String:
                            return sourceData.BoolValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.CalendarPeriodValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.CalendarPeriodValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.DateValue:
                    switch (targetType)
                    {
                        case DTEntityKind.Date:
                            return sourceData.DateValue;
                        case DTEntityKind.String:
                            return sourceData.DateValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.DayOfWeekValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.DayOfWeekValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.EnumValue:
                    switch (targetType)
                    {
                        case DTEntityKind.Integer:
                            return sourceData.EnumValue;
                        case DTEntityKind.String:
                            return sourceData.EnumValue.ToString();
                    }
                    break;
                 case TypedValue.ValueOneofCase.Float32Value:
                    switch (targetType)
                    {
                        case DTEntityKind.Boolean:
                            if (sourceData.Float32Value == 1 || sourceData.Float32Value == 0)
                            {
                                return sourceData.Float32Value == 1;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.Float32Value}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Double:
                            var convertDouble = Convert.ToDouble(sourceData.Float32Value);
                            if (double.IsFinite(convertDouble))
                            {
                                return convertDouble;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.Float32Value}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Duration:
                            return TimeSpan.FromHours(sourceData.Float32Value);
                        case DTEntityKind.Float:
                            if (float.IsFinite(sourceData.Float32Value))
                            {
                                return sourceData.Float32Value;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.Float32Value}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.String:
                            return sourceData.Float32Value.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.Float64Value:
                    switch (targetType)
                    {
                        case DTEntityKind.Double:
                            if (Double.IsFinite(sourceData.Float64Value))
                            {
                                return sourceData.Float64Value;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.Float64Value}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.String:
                            return sourceData.Float64Value.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.GeojsonValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.GeojsonValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.GeopointValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.GeopointValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.Int32Value:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.Int32Value.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.Int64Value:
                    switch (targetType)
                    {
                        case DTEntityKind.Long:
                            return sourceData.Int64Value;
                        case DTEntityKind.String:
                            return sourceData.Int64Value.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.IntervalValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.IntervalValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.MoneyValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.MoneyValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.MonthValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.MonthValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.PhoneNumberValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.PhoneNumberValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.PostalAddressValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.PostalAddressValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.StringValue:
                    switch (targetType)
                    {
                        case DTEntityKind.Boolean:
                            bool outputBool = false;
                            if (sourceData.StringValue == "1" || sourceData.StringValue == "0" || Boolean.TryParse(sourceData.StringValue, out outputBool))
                            {
                                return outputBool || sourceData.StringValue == "1";
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Date:
                            if (DateTime.TryParse(sourceData.StringValue, out var outputDate))
                            {
                                //TODO: Is there a built in dotnet way to get ISO8601 dates?
                                return outputDate.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.DateTime:
                            if (DateTime.TryParse(sourceData.StringValue, out var outputDateTime))
                            {
                                return outputDateTime;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Double:
                            if (Double.TryParse(sourceData.StringValue, out var outputDouble) && Double.IsFinite(outputDouble))
                            {
                                return outputDouble;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Duration:
                            if (TimeSpan.TryParse(sourceData.StringValue, out var outputTimeSpan))
                            {
                                return outputTimeSpan;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Float:
                            if (float.TryParse(sourceData.StringValue, out var outputFloat) && float.IsFinite(outputFloat))
                            {
                                return outputFloat;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Integer:
                            if (int.TryParse(sourceData.StringValue, out var outputInteger))
                            {
                                return outputInteger;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Long:
                            if (long.TryParse(sourceData.StringValue, out var outputLong))
                            {
                                return outputLong;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.String:
                            return sourceData.StringValue.ToString();
                        case DTEntityKind.Time:
                            if (DateTime.TryParse(sourceData.StringValue, out var outputTime))
                            {
                                //TODO: Is there a built in dotnet way to get ISO8601 times?
                                return outputTime.ToString("hh:mm:ss.FFFFFFF");
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.StringValue}' to {targetType} from {sourceData.ValueCase}");
                            }
                    }
                    break;
                case TypedValue.ValueOneofCase.TimestampValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.TimestampValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.TimeValue:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.TimeValue.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.Uint32Value:
                    switch (targetType)
                    {
                        case DTEntityKind.Boolean:
                            if (sourceData.Uint32Value == 1 || sourceData.Uint32Value == 0)
                            {
                                return sourceData.Uint32Value == 1;
                            }
                            else
                            {
                                throw new InvalidCastException($"Failed to cast '{sourceData.Uint32Value}' to {targetType} from {sourceData.ValueCase}");
                            }
                        case DTEntityKind.Double:
                            return Convert.ToDouble(sourceData.Uint32Value);
                        case DTEntityKind.String:
                            return sourceData.Uint32Value.ToString();
                    }
                    break;
                case TypedValue.ValueOneofCase.Uint64Value:
                    switch (targetType)
                    {
                        case DTEntityKind.String:
                            return sourceData.Uint64Value.ToString();
                    }
                    break;
                default:
                    logger.LogError("Uncertain how to cast from {sourceType}", sourceData.ValueCase.ToString());
                    throw new NotImplementedException($"Uncertain how to cast from {sourceData.ValueCase}");
            }

            logger.LogError("Uncertain how to cast from {sourceType} to {targetType}", sourceData.ValueCase.ToString(), targetType.ToString());
            throw new NotImplementedException($"Uncertain how to cast from {sourceData.ValueCase} to {targetType}");
        }
    }
}
