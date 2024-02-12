namespace WebX.Security.Shared.EnumTypes
{
    public enum VerifyActionType
    {
        SIGNIN = 1,
        SIGNUP = 2,
        PASSWORDRESET = 4,
        TOTP = 8,
        AUTHENTICATEWALLET = 16
    }
}
