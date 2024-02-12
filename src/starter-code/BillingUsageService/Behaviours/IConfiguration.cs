using Microsoft.Extensions.Configuration;

namespace BillingUsageService.Behaviours
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
