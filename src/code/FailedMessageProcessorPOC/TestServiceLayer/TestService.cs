using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using Microsoft.Extensions.Logging;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Settings;
using TestServiceLayer.Shared.Types;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace TestServiceLayer
{
    public class TestService : ITestService
    {
        private readonly EventRuleSettings _eventRuleSettings;
        private readonly DLQSettings _dlqSettings;
        private readonly IEventbridgeWrapper _eventbridgeWrapper;
        private readonly ISqsWrapper _sqsWrapper;
        private readonly ILogger<TestService> _logger;

        public TestService(EventRuleSettings eventRuleSettings,
            DLQSettings dlqSettings,
            IEventbridgeWrapper eventbridgeWrapper,
            ISqsWrapper sqsWrapper,
            ILogger<TestService> logger) {
            _eventRuleSettings = eventRuleSettings;
            _dlqSettings = dlqSettings;
            _eventbridgeWrapper = eventbridgeWrapper;
            _sqsWrapper = sqsWrapper;
            _logger = logger;
        }


        public async Task HandleEventRule(EventRuleDetail eventRuleDetail)
        {
            try
            {
                _logger.LogInformation("settings {@_eventRuleSettings}", _eventRuleSettings);

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
            
        }

        public async Task SubmitEventBusEntry(string eventBusEntryJson)
        {
            try
            {
                _logger.LogInformation("settings {@_eventRuleSettings}", _eventRuleSettings);
                var eventBusMessage = JsonSerializer.Deserialize<EventBusMessage>(eventBusEntryJson);

                if (eventBusMessage != null)
                {
                    await HandleEventRule(eventBusMessage.Detail!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
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

        public async Task ProcessDLQMessages()
        {
            var messageRetrievedCount = 0;

            while (messageRetrievedCount < _dlqSettings.BatchSize)
            {
                if (string.IsNullOrEmpty(_dlqSettings.QueueName))
                {
                    _logger.LogWarning("_dlqSettings.QueueName is empty in TestService");
                    break;
                }

                // Retrieve queue message
                var messageFromQueue = await _sqsWrapper.GetMessage(_dlqSettings.QueueName!);

                // if message is null break;
                if (messageFromQueue == null)
                {
                    _logger.LogWarning("ProcessDLQMessages->GetMessage returned empty stopping processing");
                    break;
                }

                try
                {
                    // Place message back in event bus
                    await SubmitEventBusEntry(messageFromQueue.MessageBody!);

                    // delete message
                    await _sqsWrapper.DeleteMessage(messageFromQueue.ReceiptHandle!, _dlqSettings.QueueName!);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in ProcessDLQMessages", ex);
                }
            }
        }
    }
}
