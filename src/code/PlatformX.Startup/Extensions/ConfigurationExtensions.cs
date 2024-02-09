using Microsoft.Extensions.DependencyInjection;
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
                Prefix = Environment.GetEnvironmentVariable("PREFIX"),
                RoleKey = Environment.GetEnvironmentVariable("ROLEKEY"),
                PortalName = Environment.GetEnvironmentVariable("PORTAL_NAME"),
                ServiceName = Environment.GetEnvironmentVariable("SERVICE_NAME"),
                Layer = Environment.GetEnvironmentVariable("LAYER"),
                AzureRegion = Environment.GetEnvironmentVariable("AZURE_REGION"),
                AzureLocation = Environment.GetEnvironmentVariable("AZURE_LOCATION"),
                AzureTenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
                AwsRegion = Environment.GetEnvironmentVariable("AWS_REGION"),
                SsmPrefix = Environment.GetEnvironmentVariable("SSM_PREFIX"),
                ServiceKeys = Environment.GetEnvironmentVariable("SERVICE_KEYS"),
                RuntimeConfiguration = Environment.GetEnvironmentVariable("RUNTIME_CONFIGURATION"),
                ServiceSymmetricKeyName = Environment.GetEnvironmentVariable("SERVICE_SYMMETRIC_KEY"),
                HostEnvironmentType = Environment.GetEnvironmentVariable("HOST_ENVIRONMENT_TYPE"),
                CorsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS"),
            };

            // Check environment name strings


            services.AddSingleton(bootstrapConfiguration);

            return bootstrapConfiguration;
        }
    }
}
