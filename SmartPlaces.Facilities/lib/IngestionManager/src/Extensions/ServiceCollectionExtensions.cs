// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.AzureDigitalTwins;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;

    /// <summary>
    /// Static extension method class for adding an ingestion manager onto a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a SmartPlaces.Facilities ingestion manager to an IServiceCollection for DI-based deployment.
        /// </summary>
        /// <typeparam name="TOptions">Ingestion manager options type.</typeparam>
        /// <param name="services">Collection of service descriptors to which Ingestion Manager will be added.</param>
        /// <param name="options">Ingestion manager options.</param>
        /// <returns>Collection of service descriptors to which Ingestion Manager has been added.</returns>
        public static IServiceCollection AddIngestionManager<TOptions>(this IServiceCollection services, Action<TOptions> options)
            where TOptions : IngestionManagerOptions
        {
            services.AddOptions<TOptions>()
                    .Configure(options)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddHttpClient("Microsoft.SmartPlaces.Facilities", options =>
            {
                options.DefaultRequestHeaders.Add("ms-smartfacilities-version", Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0");
            });

            services.AddSingleton<IGraphNamingManager, DefaultGraphNamingManager>();
            services.AddSingleton<IOutputGraphManager, AzureDigitalTwinsGraphManager<TOptions>>();
            return services;
        }
    }
}
