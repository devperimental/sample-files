using Microsoft.Extensions.Configuration;

namespace EventRuleLambda.Behaviours
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
