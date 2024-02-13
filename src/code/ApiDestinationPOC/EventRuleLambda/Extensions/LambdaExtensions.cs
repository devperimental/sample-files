using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer;
using EventRuleLambda.Behaviours;
using Helper.Startup.Extensions;
using Microsoft.Extensions.Configuration;
using TestServiceLayer.Shared.Settings;
using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.EventBridge;
using Helper.Startup.Types;

namespace EventRuleLambda.Extensions
{
    public static class TestServiceExtensions
    {
        /// <summary>
        /// AddTestService
        /// </summary>
        /// <param name="services"></param>
        public static void AddTestService(this IServiceCollection services)
        {
            services.AddBootstrapConfiguration();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddAwsResources();

            var scope = services.BuildServiceProvider();
            var bootstrapConfiguration = scope.GetRequiredService<BootstrapConfiguration>();
            var configurationService = scope.GetRequiredService<IConfigurationService>();
            var configuration = configurationService.GetConfiguration();

            services.AddLambdaService(bootstrapConfiguration);

            services.AddSingleton(typeof(EventRuleSettings), c =>
            {
                var config = new EventRuleSettings { };
                configuration.GetSection(nameof(EventRuleSettings)).Bind(config);
                
                return config;
            });
            services.AddTransient<IEventbridgeWrapper, EventbridgeWrapper>();
            services.AddSingleton<ITestService, TestService>();
        }
    }
}
