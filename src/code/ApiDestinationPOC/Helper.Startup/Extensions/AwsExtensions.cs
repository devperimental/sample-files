using Amazon;
using Amazon.EventBridge;
using Amazon.S3;
using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;

namespace Helper.Startup.Extensions
{
    public static class AwsExtensions
    {
        public static void AddAwsResources(this IServiceCollection services)
        {
            services.AddS3Client();
            services.AddSqsClient();
            services.AddEventbridgeClient();
        }

        public static void AddS3Client(this IServiceCollection services)
        {
            services.AddSingleton(typeof(AmazonS3Client), c => {
                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonS3Client(region);
            });
        }

        public static void AddSqsClient(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IAmazonSQS), c => {
                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonSQSClient(region);
            });
        }

        public static void AddEventbridgeClient(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IAmazonEventBridge), c => {
                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonEventBridgeClient(region);
            });
        }
    }
}