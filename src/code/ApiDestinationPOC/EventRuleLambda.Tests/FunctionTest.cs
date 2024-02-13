using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.SQSEvents;
using Moq.AutoMock;
using TestServiceLayer.Shared.Behaviours;
using Moq;
using TestServiceLayer.Shared.Types;
using System.Text.Json;

namespace EventRuleLambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestSQSEventLambdaFunction()
    {
        var mocker = new AutoMocker();

        var testService = mocker.GetMock<ITestService>();
        testService.Setup(c=>c.HandleEventRule(It.IsAny<EventRuleDetail>())).Returns(Task.CompletedTask);

        var eventRuleId = Guid.NewGuid().ToString();

        var eventRule = new EventRuleDetail
        {
            RequestId = eventRuleId,
            IdempotencyKey = Guid.NewGuid().ToString(),
            PartnerName = "TestPartner",
        };

        var serializedData = JsonSerializer.Serialize(eventRule); // Use in the mock lambda test tool // "{\"RequestId\":\"7f8a6647-2687-48cb-bfa1-fa259503dcde\",\"IdempotencyKey\":\"e01b8716-f2bf-4188-8e27-52882a093c37\",\"PartnerName\":\"TestPartner\"}"

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = serializedData
                }
            }
        };

        var logger = new TestLambdaLogger();
        var context = new TestLambdaContext
        {
            Logger = logger
        };


        var function = new Function(testService.Object);
        await function.FunctionHandler(sqsEvent, context);

        testService.Verify(c => c.HandleEventRule(It.IsAny<EventRuleDetail>()));

        Assert.Contains(eventRuleId, logger.Buffer.ToString());
    }
}