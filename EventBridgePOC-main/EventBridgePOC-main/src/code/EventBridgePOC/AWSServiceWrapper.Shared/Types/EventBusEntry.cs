using System;

namespace AWSServiceWrapper.Shared.Types
{
    public class EventBusEntry
    {
        public string? Source { get; set; }
        public string? Detail { get; set; }
        public string? DetailType { get; set; }
    }
}
