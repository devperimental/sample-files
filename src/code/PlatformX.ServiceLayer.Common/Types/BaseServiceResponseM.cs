using PlatformX.ServiceLayer.Common.Behaviours;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlatformX.ServiceLayer.Common.Types
{
    public class BaseServiceResponseM : IBaseServiceResponseM
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? MessageId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? CorrelationId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? IdempotencyToken { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool InError { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? ErrorType { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<string>? Messages { get; set; }
    }
}
