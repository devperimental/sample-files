namespace WebX.Security.Shared.Constants
{
    public static class VerificationFailureReason
    {
        public const string NONE = nameof(NONE);
        public const string DAILYLIMITEXCEEDED = nameof(DAILYLIMITEXCEEDED);
        public const string CODEMISMATCH = nameof(CODEMISMATCH);
        public const string RETRYLIMITEXCEEDED = nameof(RETRYLIMITEXCEEDED);
        public const string CODEEXPIRED = nameof(CODEEXPIRED);
        public const string EMAILALREADYCONFIRMED = nameof(EMAILALREADYCONFIRMED);
        public const string MOBILEALREADYCONFIRMED = nameof(MOBILEALREADYCONFIRMED);
        public const string ALTERNATEEMAILALREADYCONFIRMED = nameof(ALTERNATEEMAILALREADYCONFIRMED);
        public const string EMAILNOTFOUNDFOURUSER = nameof(EMAILNOTFOUNDFOURUSER);
        public const string MOBILENOTFOUNDFOURUSER = nameof(MOBILENOTFOUNDFOURUSER);
        public const string ALTERNATEEMAILNOTFOUNDFOURUSER = nameof(ALTERNATEEMAILNOTFOUNDFOURUSER);
    }
}
