using TestServiceLayer.Shared.Types;
using System.Threading.Tasks;

namespace TestServiceLayer.Shared.Behaviours
{
    public interface ITestService
    {
        Task HandleEventRule(EventRuleDetail eventRuleDetail);
    }
}
