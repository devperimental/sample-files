namespace WebX.Security.Shared.Types
{
   public class JwtClaimContext
   {
        public string? IdentityId { get; set; }
        public string? OrganisationGlobalId { get; set; }
        public string? UserGlobalId { get; set; }
        public string? SessionId { get; set; }
        public string? PortalName { get; set; }
        public string? SignupRequirement { get; set; }
        public string? SignupState { get; set; }
        public string? SignupScope { get; set; }
    }
}
