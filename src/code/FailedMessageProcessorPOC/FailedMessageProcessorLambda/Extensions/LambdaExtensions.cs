using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer;
using FailedMessageProcessorLambda.Behaviours;
using Helper.Startup.Extensions;
using Microsoft.Extensions.Configuration;
using TestServiceLayer.Shared.Settings;
using Helper.Startup.Types;
using Amazon.EventBridge;
using Amazon;
using AWSServiceWrapper.EventBridge;
using AWSServiceWrapper.Sqs;
using AWSServiceWrapper.Shared.Behaviours;
using Amazon.SQS;

namespace FailedMessageProcessorLambda.Extensions
{
    public static class TestServiceExtensions
    {
        /// <summary>
        /// AddTestService
        /// </summary>
        /// <param name="services"></param>
        public static void AddTestService(this IServiceCollection services)
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

            services.AddSingleton(typeof(DLQSettings), c =>
            {
                var config = new DLQSettings { };
                configuration.GetSection(nameof(DLQSettings)).Bind(config);
                return config;
            });

            services.AddTransient(typeof(IAmazonEventBridge), c =>
            {
                var accessKey = bootstrapConfiguration.AwsAccessKey;
                var accessSecret = bootstrapConfiguration.AwsAccessSecret;
                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, accessSecret);

                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                
                return new AmazonEventBridgeClient(awsCredentials, region);
            });

            services.AddTransient(typeof(IAmazonSQS), c =>
            {
                var accessKey = bootstrapConfiguration.AwsAccessKey;
                var accessSecret = bootstrapConfiguration.AwsAccessSecret;
                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, accessSecret);

                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);

                return new AmazonSQSClient(awsCredentials, region);
            });

            services.AddEventbridgeClient();
            services.AddTransient<IEventbridgeWrapper, EventbridgeWrapper>();
            services.AddTransient<ISqsWrapper, SqsWrapper>();
            services.AddTransient<ITestService, TestService>();
        }
    }
}
