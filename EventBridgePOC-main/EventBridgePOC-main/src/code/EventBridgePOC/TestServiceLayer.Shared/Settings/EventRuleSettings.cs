using System;
using System.Collections.Generic;
using System.Text;

namespace TestServiceLayer.Shared.Settings
{
    public class EventRuleSettings
    {
        public string? EventSource { get; set; }
        public string? EventDetailType { get; set; }
        public string? EventRuleQUrl { get; set; }
        public string? EventRuleDLQUrl { get; set; }
    }
}
