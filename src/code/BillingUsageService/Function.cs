using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.Portal.Shared;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BillingUsageService;

public class Function
{
    private IPortalService _portalService;
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        Startup.ConfigureServices();

        _portalService = Startup.Services!.GetRequiredService<IPortalService>();
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
        var externalIpAddress = GetExternalIpAddress(context);
        
        if (externalIpAddress != null)
        {
            context.Logger.LogInformation($"external ip address is {externalIpAddress}");
        }

        if (evnt == null)
        {
            context.Logger.LogInformation($"event is null exiting handler");
            return;
        }

        if (evnt.Records == null || evnt.Records.Count == 0)
        {
            context.Logger.LogWarning($"event contains zero records - exiting handler");
            return;
        }

        foreach (var record in evnt.Records)
        {
            await ProcessRecordAsync(record, context);
        }
    }

    private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing record: {record.Sns.MessageId}");
            var response = await _portalService.ProcessUsageRecords();
            context.Logger.LogInformation($"Message={response.Message} Success={response.Success} FailureReason={response.FailureReason}");
            context.Logger.LogInformation($"Processed record: {record.Sns.MessageId}");
        }
        catch(Exception ex)
        {
            context.Logger.LogError(ex.Message);
        }
    }

    private async Task<string> GetExternalIpAddress(ILambdaContext context)
    {
        var ipAddress = string.Empty;

        try
        {
            using var client = new HttpClient();
            ipAddress = await client.GetStringAsync(new Uri("https://ipv4.icanhazip.com/")).ConfigureAwait(true);
        }
        catch(Exception ex)
        {
            context.Logger.LogError(ex.Message);
        }

        return ipAddress;
    }
}