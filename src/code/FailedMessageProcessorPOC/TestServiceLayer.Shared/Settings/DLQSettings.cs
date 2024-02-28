namespace TestServiceLayer.Shared.Settings
{
    public class DLQSettings
    {
        public string? QueueName { get; set; }
        public int BatchSize { get; set; }
    }
}
