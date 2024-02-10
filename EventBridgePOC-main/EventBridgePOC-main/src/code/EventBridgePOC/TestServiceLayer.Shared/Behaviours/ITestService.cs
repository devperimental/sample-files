using TestServiceLayer.Shared.Types;
using System;
using System.Threading.Tasks;

namespace TestServiceLayer.Shared.Behaviours
{
    public interface ITestService
    {
        Task HandleEventRule(EventRuleDetail eventRuleDetail);
    }
}
