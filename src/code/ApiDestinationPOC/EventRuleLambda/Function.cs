using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Types;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EventRuleLambda;

public class Function
{
    private ITestService _testService;
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        Startup.ConfigureServices();

        _testService = Startup.Services!.GetRequiredService<ITestService>();
    }

    public Function(ITestService testService)
    {
        _testService = testService;
    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Starting processing of message {message.MessageId}");
        
        var eventRuleDetail = JsonSerializer.Deserialize<EventRuleDetail>(message.Body);

        if (eventRuleDetail != null)
        {
            await _testService.HandleEventRule(eventRuleDetail);
            context.Logger.LogInformation($"Finished processing of message {message.MessageId} with body {message.Body}");
        }
        else
        {
            context.Logger.LogWarning($"Unable to desrialize message {message.MessageId} with body {message.Body}");
        }

        await Task.CompletedTask;
    }
}