namespace WebX.Security.Shared.Types
{
    public class AuthStateM
    {
        public int SignupRequirement { get; set; }
        public int SignupState { get; set; }
        public string? SignupScope { get; set; }
        public string? SigninScope { get; set; } // COMPLETE - REQUIREMFA
        public int MFAChoices { get; set; } 
    }
}
