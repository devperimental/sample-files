using FailedMessageProcessorLambda.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace FailedMessageProcessorLambda
{
    public static class Startup
    {
        public static ServiceProvider? Services { get; private set; }
        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTestService();

            Services = serviceCollection.BuildServiceProvider();
        }
    }
}
