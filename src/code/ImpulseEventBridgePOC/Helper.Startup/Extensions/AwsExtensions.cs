using Amazon;
using Amazon.EventBridge;
using Microsoft.Extensions.DependencyInjection;

namespace Helper.Startup.Extensions
{
    public static class AwsExtensions
    {
        public static void AddEventbridgeClient(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IAmazonEventBridge), c => {
                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonEventBridgeClient(region);
            });
        }
    }
}