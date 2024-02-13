using Microsoft.Extensions.Logging;
using TestServiceLayer.Shared.Behaviours;
using System.Threading.Tasks;

namespace TestServiceLayer
{
    public class TestCallback : ITestCallback
    {
        private readonly ILogger<TestCallback> _logger;

        public TestCallback(ILogger<TestCallback> logger) {
            _logger = logger;
        }

        public async Task HandleCallback(string bodyJson, string partnerName, string idempotencyKey, string requestId)
        {
            _logger.LogInformation("Callback received values {bodyJson} {partnerName} {idempotencyKey} {requestId}", bodyJson, partnerName, idempotencyKey, requestId);

            await Task.CompletedTask;
        }
    }
}
