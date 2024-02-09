using System.Collections.Generic;

namespace PlatformX.ServiceLayer.Common.Behaviours
{
    public interface IBaseServiceResponseM
    {
        public string? MessageId { get; set; }
        public string? CorrelationId { get; set; }
        public string? IdempotencyToken { get; set; }
        public bool InError { get; set; }
        public string? ErrorType { get; set; }
        public List<string>? Messages { get; set; }
    }
}
