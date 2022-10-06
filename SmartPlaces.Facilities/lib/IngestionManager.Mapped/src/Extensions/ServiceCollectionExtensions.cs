// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IngestionManager.Mapped.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using IngestionManager.Extensions;
    using IngestionManager.Interfaces;
    using IngestionManager.Mapped;

    public static class ServiceCollectionExtensions
    {
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
