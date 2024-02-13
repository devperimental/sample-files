using Microsoft.Extensions.DependencyInjection;
using Helper.Startup.Types;

namespace Helper.Startup.Extensions
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
                CorsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS"),
                AddHealthCheck = Environment.GetEnvironmentVariable("ADD_HEALTHCHECK") == "true",
            };

            services.AddSingleton(bootstrapConfiguration);

            return bootstrapConfiguration;
        }
    }
}
