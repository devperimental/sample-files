using AWSServiceWrapper.Shared.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSServiceWrapper.Shared.Behaviours
{
    public interface ISqsWrapper
    {
        Task<string> SendMessage(
            string queueUrl,
            string messageBody,
            Dictionary<string, WrapperMessageAttributeValue> wrapperMessageAttributes);

        Task<MappedSqsMessage> GetMessage(string queueUrl);

        Task DeleteMessage(string receiptHandle, string queueUrl);
    }
}
