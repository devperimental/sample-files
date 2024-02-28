using Helper.Startup.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Types;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
        services.AddConsoleService()
    ) ;

using IHost host = builder.Build();

await RunHarness(host.Services);

await host.RunAsync();

static async Task RunHarness(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    
    var testService = provider.GetRequiredService<ITestService>();

    for(var i=0; i<10; i++)
    {
        var eventRuleDetail = new EventRuleDetail
        {
            RequestId = Guid.NewGuid().ToString(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            PartnerName = "PartnerConsoleApp"
        };

        await testService.HandleEventRule(eventRuleDetail);


    }

    Console.WriteLine();
}
