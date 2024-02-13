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
        private readonly EventRuleSettings _eventRuleSettings;
        private readonly IEventbridgeWrapper _eventbridgeWrapper;
        private readonly ILogger<TestService> _logger;

        public TestService(EventRuleSettings eventRuleSettings, 
            IEventbridgeWrapper eventbridgeWrapper, 
            ILogger<TestService> logger) {
            _eventRuleSettings = eventRuleSettings;
            _eventbridgeWrapper = eventbridgeWrapper;
            _logger = logger;
        }


        public async Task HandleEventRule(EventRuleDetail eventRuleDetail)
        {
            var eventBusEntry = new EventBusEntry
            {
                DetailType = _eventRuleSettings.EventDetailType,
                Detail = JsonSerializer.Serialize(eventRuleDetail),
                EventBusName = _eventRuleSettings.EventBusName,
                Source = _eventRuleSettings.EventSource
            };

            var response = await _eventbridgeWrapper.PutCustomEvent(eventBusEntry);

            if (!response)
            {
                _logger.LogWarning("Unable to submit event for {@eventRuleDetail}", eventRuleDetail);
            }
        }

        /*
         {
              "version": "0",
              "id": "9688da20-683b-3e58-921f-fbc9722f6d5f",
              "detail-type": "testDetailType",
              "source": "com.test.it",
              "account": "301804962855",
              "time": "2024-02-13T03:09:16Z",
              "region": "ap-southeast-2",
              "resources": [],
              "detail": {
                "idempotencyKey": "ABCDEFHG",
                "requestId": "HIJKLMOPQ"
              }
            } 
         * */
    }
}
