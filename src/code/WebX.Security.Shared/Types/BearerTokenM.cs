using System;

namespace WebX.Security.Shared.Types
{
    public class BearerTokenM
    {
        public string? TokenValue { get; set; }
        public DateTime TokenExpiryUTC { get; set; }
    }
}
