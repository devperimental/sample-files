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
            ID = eventRuleId
        };

        var sqsEvent = new SQSEvent
        {
            Records = new List<SQSEvent.SQSMessage>
            {
                new SQSEvent.SQSMessage
                {
                    Body = JsonSerializer.Serialize(eventRule)
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