//-----------------------------------------------------------------------
// <copyright file="TelemetryProcessor.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace EdgeGateway
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Google.Protobuf;
    using Mapped.Gateway;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;
    using StackExchange.Redis;

    public class TelemetryProcessor : BackgroundService
    {
        private readonly DeviceClient deviceClient;
        private readonly IConnectionMultiplexer redis;
        private readonly ILogger<TelemetryProcessor> logger;
        private readonly TelemetryClient telemetryClient;
        private readonly AsyncRetryPolicy iotHubRetryPolicy;
        private readonly AsyncRetryPolicy redisRetryPolicy;
        private readonly Random jitter;

        private const string MetricsNamespace = "EdgeGateway";
        private MetricIdentifier messageProcessed = new MetricIdentifier(MetricsNamespace, "MessageProcessed", "Status", "Reason");

        public TelemetryProcessor(ILogger<TelemetryProcessor> logger,
                                    IConfiguration config,
                                    IConnectionMultiplexer redis,
                                    TelemetryClient telemetryClient,
                                    DeviceClient deviceClient)
        {
            this.logger = logger;
            this.deviceClient = deviceClient;
            this.redis = redis;
            this.telemetryClient = telemetryClient;

            logger.LogInformation("KeyVaultEndpoint: {AzureDigitalTwinsEndpoint}", config["KeyVaultEndpoint"]);
            logger.LogInformation("Redis Status: {status}", redis.GetStatus());

            var iotHubMaxRetryAttempts = 3;
            jitter = new Lazy<Random>().Value;
            iotHubRetryPolicy = Policy.Handle<IotHubCommunicationException>(ex => ex.Message == "Transient network error occurred, please retry. Failed to write Connection reset by peer")
                                      .Or<InvalidOperationException>(ex => ex.Message == "Invalid transport state: Closed")
                                      .WaitAndRetryAsync(iotHubMaxRetryAttempts,
                                                        (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)),
                                                        (ex, waitTime, retryAttepmt, context) =>
                                                        {
                                                            logger.LogWarning("Failed attempt {retryAttempt} of {maxRetryAttempts} to send message to IotHub. Trying again in {waitTime} seconds.", retryAttepmt, iotHubMaxRetryAttempts, waitTime.TotalSeconds);
                                                        });

            var redisMaxRetryAttempts = 3;
            redisRetryPolicy = Policy.Handle<RedisTimeoutException>()
                                     .WaitAndRetryAsync(redisMaxRetryAttempts,
                                                        (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 100)),
                                                        (ex, waitTime, retryAttepmt, context) =>
                                                        {
                                                            logger.LogWarning("Failed attempt {retryAttempt} of {maxRetryAttempts} to communicate with Redis. Trying again in {waitTime} seconds.", retryAttepmt, redisMaxRetryAttempts, waitTime.TotalSeconds);
                                                        });
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting to process Telemetry");

            // Connect to Redis
            var db = redis.GetDatabase((int)RedisDatabase.Telemetry);

            // Subscribe to changes...
            var channel = new RedisChannel($"__keyevent@{RedisDatabase.Telemetry:D}__:set", RedisChannel.PatternMode.Pattern);
            var subscriber = redis.GetSubscriber();

            // Using OnMessage will process events sequentially, preserving order, but slower
            // Using the action handler in Subscribe(Async) will process concurrently making no guarantee on order
            // https://stackexchange.github.io/StackExchange.Redis/PubSubOrder
            await subscriber.SubscribeAsync(channel, async (channel, data) =>
            {
                var status = "Failed";
                var reason = "Unknown";
                try
                {
                    var mappingKey = data.ToString();

                    // Fetch the new value
                    var value = await redisRetryPolicy.ExecuteAsync(async () => await db.StringGetAsync(mappingKey));

                    // Peek at the protobuf otherwise observed network issues will significantly increase required IotHub capacity
                    var point = RedisPoint.Parser.ParseFrom(value);

                    // Write the value to the logs
                    logger.LogDebug("{mappingKey}: {point}", mappingKey, point.ToString());

                    if (point.ConsecutiveNetworkErrorCount == 0)
                    {
                        using var hubMessage = new Message(value)
                        {
                            ContentType = "application/grpc",
                        };

                        hubMessage.Properties.Add("mappingKey", mappingKey);

                        await iotHubRetryPolicy.ExecuteAsync(async () => await deviceClient.SendEventAsync(hubMessage));

                        status = "Succeeded";
                        reason = "Sent";
                    }
                    else
                    {
                        logger.LogWarning("Point encountered edge network issue. MappingKey: {mappingKey} LastSeen: {lastSeen} CommunicationAttempts: {communicationAttempts}", mappingKey, point.NetworkErrorStart, point.ConsecutiveNetworkErrorCount);
                        status = "Dropped";
                        reason = "DeviceNetworkIssue";
                    }
                }
                catch (InvalidProtocolBufferException ex)
                {
                    logger.LogError(ex, "InvalidProtocolBufferException: {message}", ex.Message);
                    reason = "DecodingProtobof";
                }
                catch (IotHubException ex)
                {
                    logger.LogError(ex, "Error sending message to IotHub.");
                    reason = "IotHub";
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unknown Error: {message}", ex.Message);
                    reason = ex.GetType().ToString();
                }
                finally
                {
                    telemetryClient.GetMetric(messageProcessed).TrackValue(1, status, reason);
                    logger.LogDebug("Message from Redis processed. {result}", status);
                }
            });
        }
    }
}
