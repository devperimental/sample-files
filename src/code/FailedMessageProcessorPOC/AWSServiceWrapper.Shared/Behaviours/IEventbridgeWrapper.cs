using AWSServiceWrapper.Shared.Types;
using System.Threading.Tasks;

namespace AWSServiceWrapper.Shared.Behaviours
{
    public interface IEventbridgeWrapper
    {
        Task<bool> PutCustomEvent(EventBusEntry eventBusEntry);
    }
}
