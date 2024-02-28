using System.Text.Json.Serialization;

namespace TestServiceLayer.Shared.Types
{
    public class EventRuleDetail
    {
        public string? RequestId { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? PartnerName { get; set; }
    }

    public class EventBusMessage
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("version")] 
        public string? Version { get; set; }
        [JsonPropertyName("detail-type")] 
        public string? DetailType { get; set; }
        [JsonPropertyName("source")] 
        public string? Source { get; set; }
        [JsonPropertyName("account")] 
        public string? Account { get; set; }
        [JsonPropertyName("time")] 
        public string? Time { get; set; }
        [JsonPropertyName("region")] 
        public string? Region { get; set; }
        [JsonPropertyName("detail")] 
        public EventRuleDetail? Detail { get; set; }
    }
}
