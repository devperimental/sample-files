using Microsoft.Extensions.DependencyInjection;

namespace BillingUsageService
{
    public static class Startup
    {
        public static ServiceProvider? Services { get; set; } 

        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddBillableItemServiceLayer();

            Services = serviceCollection.BuildServiceProvider();
        }
    }
}
