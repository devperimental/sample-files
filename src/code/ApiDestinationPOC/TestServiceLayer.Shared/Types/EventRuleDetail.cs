namespace TestServiceLayer.Shared.Types
{
    public class EventRuleDetail
    {
        public string? RequestId { get; set; }
        public string? IdempotencyKey { get; set; }
        public string? PartnerName { get; set; }
    }
}
