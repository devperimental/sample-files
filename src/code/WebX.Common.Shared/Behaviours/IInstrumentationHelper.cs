using PlatformX.ServiceLayer.Common.Behaviours;
using System;
using System.Threading.Tasks;

namespace WebX.Common.Shared.Behaviours
{
    public interface IInstrumentationHelper
    {
        Task<TResponseM> InstrumentRequest<TResponseM>(Func<Task<TResponseM>> func, string controller, string actionName, long responseSize = 0) where TResponseM : IBaseServiceResponseM, new();

        Task InstrumentRequest(Action action, string controller, string actionName, long responseSize = 0);
    }
}
