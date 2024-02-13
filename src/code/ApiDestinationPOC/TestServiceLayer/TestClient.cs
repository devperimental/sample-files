using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using TestServiceLayer.Shared.Behaviours;
using TestServiceLayer.Shared.Settings;
using TestServiceLayer.Shared.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestServiceLayer
{
    public class TestClient : ITestClient
    {
        private readonly EventRuleSettings _eventRuleSettings;
        private readonly ISqsWrapper _sqsWrapper;
        private readonly ILogger<TestClient> _logger;

        public TestClient(EventRuleSettings eventRuleSettings,
            ISqsWrapper sqsWrapper,
            ILogger<TestClient> logger)
        {
            _eventRuleSettings = eventRuleSettings;
            _sqsWrapper = sqsWrapper;
            _logger = logger;
        }

        public async Task NotifyEventRule(EventRuleDetail eventRuleDetail)
        {
            var messageBody = JsonSerializer.Serialize(eventRuleDetail);

            var messageAttributes = new Dictionary<string, WrapperMessageAttributeValue>
            {
                { "RequestId",   new WrapperMessageAttributeValue { DataType = "String", StringValue = eventRuleDetail.RequestId } },
                { "IdempotencyKey",  new WrapperMessageAttributeValue { DataType = "String", StringValue = eventRuleDetail.IdempotencyKey } },
                { "PartnerName", new WrapperMessageAttributeValue { DataType = "String", StringValue = eventRuleDetail.PartnerName } },
            };

            if (string.IsNullOrEmpty(_eventRuleSettings.EventRuleQUrl))
            {
                throw new ArgumentNullException(nameof(_eventRuleSettings.EventRuleQUrl));
            }

            var messageId = await _sqsWrapper.SendMessage(_eventRuleSettings.EventRuleQUrl, messageBody, messageAttributes);

            if (string.IsNullOrEmpty(messageId))
            {
                _logger.LogWarning("Unable to submit queue event for {@eventRuleDetail}", eventRuleDetail);
            }

            _logger.LogInformation("Message submitted with {Id}", messageId);
        }
    }
}
