using Amazon.SQS;
using Amazon.SQS.Model;
using AWSServiceWrapper.Shared.Behaviours;
using AWSServiceWrapper.Shared.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServiceWrapper.Sqs
{
    public class SqsWrapper : ISqsWrapper
    {
        private readonly IAmazonSQS _amazonSQS;
        private readonly ILogger<SqsWrapper> _logger;

        /// <summary>
        /// Constructor for the IAmazonSQS wrapper.
        /// </summary>
        /// <param name="amazonSQS">The injected IAmazonSQS client.</param>
        /// <param name="logger">The injected logger for the wrapper.</param>
        public SqsWrapper(IAmazonSQS amazonSQS, ILogger<SqsWrapper> logger)

        {
            _amazonSQS = amazonSQS;
            _logger = logger;
        }

        public async Task<string> SendMessage(
            string queueUrl,
            string messageBody,
            Dictionary<string, WrapperMessageAttributeValue> wrapperMessageAttributes)
        {
            var messageId = string.Empty;

            var messageAttributes = wrapperMessageAttributes.ToDictionary(k=> k.Key, k => new MessageAttributeValue { DataType = k.Value.DataType, StringValue = k.Value.StringValue });
            
            try
            {
                var sendMessageRequest = new SendMessageRequest
                {
                    MessageAttributes = messageAttributes,
                    MessageBody = messageBody,
                    QueueUrl = queueUrl,
                };

                var response = await _amazonSQS.SendMessageAsync(sendMessageRequest);
                messageId = response.MessageId;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }

            return messageId;
        }

        public async Task<MappedSqsMessage> GetMessage(string queueUrl)
        {
            try
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 0
                };

                var sqsMessage = await _amazonSQS.ReceiveMessageAsync(receiveMessageRequest);

                if (sqsMessage == null || sqsMessage.Messages.Count == 0)
                    return default!;

                var mappedSqsMessage = new MappedSqsMessage
                {
                    MessageId = sqsMessage.Messages[0].MessageId,
                    MessageBody = sqsMessage.Messages[0].Body,
                    ReceiptHandle = sqsMessage.Messages[0].ReceiptHandle
                };

                return mappedSqsMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }

        public async Task DeleteMessage(string receiptHandle, string queueUrl)
        {
            try
            {
                await _amazonSQS.DeleteMessageAsync(queueUrl, receiptHandle);
            }
            catch(Exception ex) {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }
    }
}
