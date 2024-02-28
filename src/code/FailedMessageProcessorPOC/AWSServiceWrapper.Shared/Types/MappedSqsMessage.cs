namespace AWSServiceWrapper.Shared.Types
{
    public class MappedSqsMessage
    {
        public string? MessageId { get;set; }
        public string? MessageBody { get; set; }
        public string? ReceiptHandle { get; set; }
    }
}

