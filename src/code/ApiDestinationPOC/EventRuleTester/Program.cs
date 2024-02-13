using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Sqs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestServiceLayer;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Types;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
        services
            .AddTransient<ISqsWrapper, SqsWrapper>()
            .AddTransient<ITestClient, TestClient>()
    );

using IHost host = builder.Build();

await RunHarness(host.Services);

await host.RunAsync();

static async Task RunHarness(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    
    var policyCompletedClient = provider.GetRequiredService<ITestClient>();

    var eventRuleDetail = new EventRuleDetail
    {
        ID = Guid.NewGuid().ToString()
    };

    await policyCompletedClient.NotifyEventRule(eventRuleDetail);

    Console.WriteLine();
}
