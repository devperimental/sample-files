using Microsoft.Extensions.Configuration;

namespace FailedMessageProcessorLambda.Behaviours
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
