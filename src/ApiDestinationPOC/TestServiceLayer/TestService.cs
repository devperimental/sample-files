using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using Microsoft.Extensions.Logging;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Settings;
using TestServiceLayer.Shared.Types;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestServiceLayer
{
    public class TestService : ITestService
    {
        private readonly EventRuleSettings _policyCompletedSettings;
        private readonly IEventbridgeWrapper _eventbridgeWrapper;
        private readonly ILogger<TestService> _logger;

        public TestService(EventRuleSettings policyCompletedSettings, 
            IEventbridgeWrapper eventbridgeWrapper, 
            ILogger<TestService> logger) {
            _policyCompletedSettings = policyCompletedSettings;
            _eventbridgeWrapper = eventbridgeWrapper;
            _logger = logger;
        }


        public async Task HandleEventRule(EventRuleDetail eventRuleDetail)
        {
            var eventBusEntry = new EventBusEntry
            {
                DetailType = _policyCompletedSettings.EventDetailType,
                Detail = JsonSerializer.Serialize(eventRuleDetail),
                Source = _policyCompletedSettings.EventSource
            };

            var response = await _eventbridgeWrapper.PutCustomEvent(eventBusEntry);

            if (!response)
            {
                _logger.LogWarning("Unable to submit event for {@eventRuleDetail}", eventRuleDetail);
            }
        }
    }
}
