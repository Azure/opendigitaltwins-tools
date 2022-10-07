﻿// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IngestionManager.Extensions
{
    using IngestionManager;
    using IngestionManager.AzureDigitalTwins;
    using IngestionManager.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIngestionManager<TOptions>(this IServiceCollection services, Action<TOptions> options)
            where TOptions : IngestionManagerOptions
        {
            services.AddOptions<TOptions>()
                    .Configure(options)
                    .ValidateDataAnnotations()
                    .ValidateOnStart();

            services.AddSingleton<IOutputGraphManager, AdtGraphManager<TOptions>>();
            return services;
        }
    }
}
