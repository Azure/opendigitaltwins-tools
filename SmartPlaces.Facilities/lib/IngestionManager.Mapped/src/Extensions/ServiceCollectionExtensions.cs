// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Mapped;

    /// <summary>
    /// Static extension method class for adding an ingestion manager onto a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a Mapped ingestion manager to an IServiceCollection, for DI-based deployment.
        /// </summary>
        /// <param name="services">Collection of service descriptors to which Mapped Ingestion Manager will be added.</param>
        /// <param name="options">Mapped ingestion manager options.</param>
        /// <returns>Collection of service descriptors to which Mapped Ingestion Manager has been added.</returns>
        public static IServiceCollection AddMappedIngestionManager(this IServiceCollection services, Action<MappedIngestionManagerOptions> options)
        {
            services.AddOptions<MappedIngestionManagerOptions>()
                    .Configure(options)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddSingleton<IInputGraphManager, MappedGraphManager>();
            services.AddSingleton<IGraphIngestionProcessor, MappedGraphIngestionProcessor<MappedIngestionManagerOptions>>();

            services.AddIngestionManager(options);

            return services;
        }
    }
}
