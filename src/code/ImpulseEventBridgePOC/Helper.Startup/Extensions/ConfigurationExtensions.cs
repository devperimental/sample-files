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
                AwsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY"),
                AwsAccessSecret = Environment.GetEnvironmentVariable("AWS_ACCESS_SECRET"),
            };

            services.AddSingleton(bootstrapConfiguration);

            return bootstrapConfiguration;
        }
    }
}
