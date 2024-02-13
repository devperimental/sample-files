using AWSServiceWrapper.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AWSServiceWrapper.Shared.Behaviours
{
    public interface IEventbridgeWrapper
    {
        Task<bool> PutCustomEvent(EventBusEntry eventBusEntry);
    }
}
