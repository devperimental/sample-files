using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using Moq;
using Moq.AutoMock;
using TestServiceLayer.Shared.Types;

namespace TestServiceLayer.Tests
{
    public class TestServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestHandleEventRule()
        {
            var mocker = new AutoMocker();

            var testService = mocker.CreateInstance<TestService>();
            var eventBridgeWrapperMock = mocker.GetMock<IEventbridgeWrapper>();

            eventBridgeWrapperMock.Setup(c => c.PutCustomEvent(It.IsAny<EventBusEntry>())).ReturnsAsync(true);

            var eventRuleDetail = new EventRuleDetail { ID = Guid.NewGuid().ToString() };
            await testService.HandleEventRule(eventRuleDetail);

            eventBridgeWrapperMock.Verify(c=> c.PutCustomEvent(It.IsAny<EventBusEntry>()));
        }
    }
}