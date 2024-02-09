using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebX.Common.Shared.Behaviours;

namespace WebX.Common
{
    public class DiagnosticHelper : IDiagnosticHelper
    {
        private readonly ILogger<DiagnosticHelper> _logger;

        public DiagnosticHelper(ILogger<DiagnosticHelper> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetExternalIpAddress()
        {
            var ipAddress = string.Empty;

            try
            {
                using var client = new HttpClient();
                ipAddress = await client.GetStringAsync(new Uri("https://ipv4.icanhazip.com/")).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return ipAddress;
        }
    }
}
