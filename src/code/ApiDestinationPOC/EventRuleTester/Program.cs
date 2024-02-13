using Amazon;
using Amazon.SQS;
using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Sqs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestServiceLayer;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Settings;
using TestServiceLayer.Shared.Types;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
        services
            .AddTransient(typeof(IAmazonSQS), c => {
                var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
                var accessSecret = Environment.GetEnvironmentVariable("AWS_ACCESS_SECRET");
                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, accessSecret);

                var region = RegionEndpoint.GetBySystemName(RegionEndpoint.APSoutheast2.SystemName);
                return new AmazonSQSClient(awsCredentials, region);
            })
            .AddTransient(typeof(EventRuleSettings), c => {
                
                return new EventRuleSettings() { EventRuleQUrl = "https://sqs.ap-southeast-2.amazonaws.com/301804962855/event-rule-source-dev-q" };
            })
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

    for(var i = 0; i < 10; i++)
    {
        var eventRuleDetail = new EventRuleDetail
        {
            RequestId = Guid.NewGuid().ToString(),
            IdempotencyKey = Guid.NewGuid().ToString(),
            PartnerName = "PartnerConsoleApp"
        };

        await policyCompletedClient.NotifyEventRule(eventRuleDetail);
    }
    Console.WriteLine();
}
