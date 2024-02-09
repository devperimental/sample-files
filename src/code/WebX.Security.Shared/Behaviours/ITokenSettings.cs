namespace WebX.Security.Shared.Behaviours
{
    public interface ITokenSettings
    {
        public int TokenCallExpirySeconds { get; set; }
        public bool TokenCheckTimestamp { get; set; }
        public string? TokenAudience { get; set; }
        public string? TokenIssuer { get; set; }
        public int TokenExpiryMinutes { get; set; }
        public int TokenMaxClockSkewSeconds { get; set; }
        public string? TokenSecurityKey { get; set; }
    }
}
