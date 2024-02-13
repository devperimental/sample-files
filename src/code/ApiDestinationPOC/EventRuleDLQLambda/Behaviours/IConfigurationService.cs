using Microsoft.Extensions.Configuration;

namespace EventRuleDLQLambda.Behaviours
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
