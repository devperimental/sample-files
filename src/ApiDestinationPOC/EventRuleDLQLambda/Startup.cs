using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer;
using TestServiceLayer.Shared.Behaviours;

namespace EventRuleDLQLambda
{
    public static class Startup
    {
        public static ServiceProvider? Services { get; private set; }
        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ITestService, TestService>();

            Services = serviceCollection.BuildServiceProvider(); 
        }
    }
}
