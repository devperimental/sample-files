using System;
using System.Collections.Generic;
using System.Text;

namespace PlatformX.ServiceLayer.Common.Types
{
    public class BaseServiceRequestM
    {
        public string? IdempotencyToken { get; set; }
    }
}
