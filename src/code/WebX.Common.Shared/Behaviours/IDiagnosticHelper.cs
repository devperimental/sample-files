using System.Threading.Tasks;

namespace WebX.Common.Shared.Behaviours
{
    public interface IDiagnosticHelper
    {
        Task<string> GetExternalIpAddress();
    }
}
