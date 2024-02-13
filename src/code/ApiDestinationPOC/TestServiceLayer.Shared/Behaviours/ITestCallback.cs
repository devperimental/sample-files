using System.Threading.Tasks;

namespace TestServiceLayer.Shared.Behaviours
{
    public interface ITestCallback
    {
        Task HandleCallback(string bodyJson, string partnerName, string idempotencyKey, string requestId);
    }
}
