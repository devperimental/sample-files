using Amazon;
using Amazon.EventBridge;
using AWSServiceWrapper.EventBridge;
using AWSServiceWrapper.Shared.Behaviours;
using Helper.Startup.Behaviours;
using Helper.Startup.Services;
using Helper.Startup.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Settings;

namespace Helper.Startup.Extensions
{
    public static class ConsoleExtensions
    {
        /// <summary>
        /// AddLambdaService
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="bootstrapConfiguration"></param>
        public static void AddConsoleService(this IServiceCollection services)
        {
            services.AddSerilog();

            services.AddBootstrapConfiguration();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            var scope = services.BuildServiceProvider();
            var bootstrapConfiguration = scope.GetRequiredService<BootstrapConfiguration>();
            var configurationService = scope.GetRequiredService<IConfigurationService>();
            var configuration = configurationService.GetConfiguration();

            services.AddSingleton(typeof(EventRuleSettings), c =>
            {
                var config = new EventRuleSettings { };
                configuration.GetSection(nameof(EventRuleSettings)).Bind(config);
                return config;
            });

            services.AddTransient(typeof(IAmazonEventBridge), c =>
            {
                var accessKey = bootstrapConfiguration.AwsAccessKey; 
                var accessSecret = bootstrapConfiguration.AwsAccessSecret ;
                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, accessSecret);

                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonEventBridgeClient(awsCredentials, region);
            });
            
            services.AddEventbridgeClient();
            services.AddTransient<IEventbridgeWrapper, EventbridgeWrapper>();
            services.AddTransient<ITestService, TestService>();

        }


    }
}
