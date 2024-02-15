using Helper.Startup.Behaviours;
using Helper.Startup.Types;
using Microsoft.Extensions.Configuration;

namespace Helper.Startup.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly BootstrapConfiguration _bootstrapConfiguration;

        public ConfigurationService(BootstrapConfiguration bootstrapConfiguration)
        {
            _bootstrapConfiguration = bootstrapConfiguration;
        }

        public IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_bootstrapConfiguration.AspNetCoreEnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
