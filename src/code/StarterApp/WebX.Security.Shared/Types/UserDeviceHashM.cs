using System;

namespace WebX.Security.Shared.Types
{
    public class UserDeviceHashM
    {
        public string? Hash { get; set; }
        public DateTime ValidUntil{ get; set; }
    }
}
