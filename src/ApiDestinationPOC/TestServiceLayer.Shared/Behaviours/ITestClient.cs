using TestServiceLayer.Shared.Types;
using System.Threading.Tasks;

namespace TestServiceLayer.Shared.Behaviours
{
    public interface ITestClient
    {
        Task NotifyEventRule(EventRuleDetail eventRuleDetail);
    }
}
