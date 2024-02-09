namespace WebX.Common.Shared.Types
{
    public class ClientRequestM
    {
        public string? ApiKey { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ApplicationGlobalId { get; set; }
        public string? OrganisationGlobalId { get; set; }
        public string? ClientApplicationKey { get; set; }
        public string? ClientApiKey { get; set; }
        public string? ClientApplicationGlobalId { get; set; }
        public string? ClientAppEnvironment { get; set; }
        public int StatusCode { get; set; }
        public int SubStatusCode { get; set; }
        public string? ReasonPhrase { get; set; }
        public bool Valid { get; set; }
    }
}
