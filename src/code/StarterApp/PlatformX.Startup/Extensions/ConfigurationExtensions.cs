﻿using Microsoft.Extensions.DependencyInjection;
using PlatformX.Common.Types.DataContract;

namespace PlatformX.Startup.Extensions
{
    public static class ConfigurationExtensions
    {
        public static BootstrapConfiguration AddBootstrapConfiguration(this IServiceCollection services)
        {
            var bootstrapConfiguration = new BootstrapConfiguration
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ENVIRONMENT_NAME"),
                AspNetCoreEnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ServiceName = Environment.GetEnvironmentVariable("SERVICE_NAME"),
                SsmPrefix = Environment.GetEnvironmentVariable("SSM_PREFIX"),
                CorsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS"),
                AddHealthCheck = Environment.GetEnvironmentVariable("ADD_HEALTHCHECK") == "true",
            };

            services.AddSingleton(bootstrapConfiguration);

            return bootstrapConfiguration;
        }
    }
}
