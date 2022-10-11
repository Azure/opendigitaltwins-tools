// -----------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.IngestionManager.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.SmartPlaces.Facilities.IngestionManager;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.AzureDigitalTwins;
    using Microsoft.SmartPlaces.Facilities.IngestionManager.Interfaces;
    using System.Reflection;

    public static class ServiceCollectionExtensions
    {
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

            services.AddSingleton<IOutputGraphManager, AzureDigitalTwinsGraphManager<TOptions>>();
            return services;
        }
    }
}
