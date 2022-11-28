//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace EdgeGateway
{
    using System;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Microsoft.Azure.Devices.Client;
    using StackExchange.Redis;

    public class Program
    {
        /// <summary>
        /// Entry point into EdgeGateway
        /// </summary>
        /// <param name="args">An option for passing in configuration values, though not required</param>
        /// <exception cref="ArgumentNullException">Thrown when required configuration values are not present</exception>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Edge Gateway");

            using IHost host = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var settings = config.Build();

                    var vaultUri = settings["KeyVaultEndpoint"];

                    if (string.IsNullOrWhiteSpace(vaultUri))
                    {
                        Console.WriteLine("Environment Variable 'KeyVaultEndpoint' not found. Exiting");
                        throw new ArgumentNullException("KeyVaultEndpoint");
                    }

                    config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure ILogger
                    services.AddLogging();

                    // Configure options for ApplicationInsights
                    services.AddApplicationInsightsTelemetryWorkerService(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["AppInsightsConnectionString"];
                        options.EnableAdaptiveSampling = false;
                    });

                    // Configure option for EdgeRedis Cache
                    services.AddSingleton<IConnectionMultiplexer>(sp =>
                    {
                        return ConnectionMultiplexer.Connect(
                            new ConfigurationOptions
                            {
                                EndPoints =
                                {
                                    hostContext.Configuration["RedisEndpoint"],
                                },
                                AbortOnConnectFail = false,
                            });
                    });

                    // Configure options for communication with IotHub
                    services.AddSingleton<DeviceClient>(sp =>
                    {
                        var transportSettings = new[]
                        {
                            new AmqpTransportSettings(
                                TransportType.Amqp_Tcp_Only,
                                AmqpTransportSettings.DefaultPrefetchCount,
                                new AmqpConnectionPoolSettings()
                                {
                                    Pooling = true,
                                }),
                            new AmqpTransportSettings(
                                TransportType.Amqp_WebSocket_Only,
                                AmqpTransportSettings.DefaultPrefetchCount,
                                new AmqpConnectionPoolSettings()
                                {
                                    Pooling = true,
                                }),
                        };

                        return DeviceClient.CreateFromConnectionString(
                            hostContext.Configuration["DeviceConnectionString"],
                            transportSettings,
                            new ClientOptions()
                            {
                                SasTokenRenewalBuffer = 51,
                            });
                    });

                    // Add the main worker body
                    services.AddHostedService<TelemetryProcessor>();

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Console.WriteLine("Hi Devs");
                    }
                })
                .Build();

            // Start the host
            await host.RunAsync();

            Console.WriteLine("Shutting down Edge Gateway");
        }
    }
}