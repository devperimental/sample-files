using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Microsoft.Extensions.DependencyInjection;
using TestServiceLayer.Shared.Behaviours;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FailedMessageProcessorLambda;

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
    /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used 
    /// to respond to SNS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
    {
        foreach(var record in evnt.Records)
        {
            await ProcessRecordAsync(record, context);
        }
    }

    private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation($"Starting processing for message {record.Sns.MessageId}");

        await _testService.ProcessDLQMessages();

        context.Logger.LogInformation($"Finished processing for message {record.Sns.MessageId}");
    }
}