namespace PlatformX.ServiceLayer.Common.Types
{
    public class ClientApplicationM
    {
        public string? ClientApiKey { get; set; }
        public string? ClientApplicationKey { get; set; }
        public string? ClientApplicationGlobalId { get; set; }
        public string? ClientAppEnvironment { get; set; }
        public string? ClientAppRegion { get; set; }
        public string? ClientAppLocation { get; set; }
        public string? OrganisationGlobalId { get; set; }
        public string? ApplicationTokenApiKeyName { get; set; }
        public string? ApplicationTokenApiKeySecret { get; set; }
        public string? ApplicationTokenApiKeyNameHash { get; set; }
        public string? ApplicationTokenApiKeySecretHash { get; set; }
        public bool BackChannelEnabled { get; set; }
        public bool FrontChannelEnabled { get; set; }
        public bool ValidateIp { get; set; }
        public bool ValidateBrowser { get; set; }
        public int TokenExpiryMinutes { get; set; }
        public int SignupRequirement { get; set; }
        public int MFAChoices { get; set; }
        public bool MFAEnabled { get; set; }
        public string? RefererList { get; set; }
        public int StatusCode { get; set; }
        public int SubStatusCode { get; set; }
        public string? ReasonPhrase { get; set; }
        public bool Valid { get; set; }
    }
}
