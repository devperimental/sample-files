namespace WebX.Security.Shared.Types
{
    public class TokenContextData
    {
        public string? PortalName { get; set; }
        public string? OrganisationGlobalId { get; set; }
        public string? UserGlobalId { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool AlternateEmailConfirmed { get; set; }
        public string? IdentityId { get; set; }
        public string? SessionId { get; set; }
        public string? ClientApplicationKey { get; set; }
        public int MFAChoices { get; set; }
        public bool MFAEnabled { get; set; }
        public int SignupRequirement { get; set; }
        public string? PlanGlobalId { get; set; }
        public string? ActiveSubscription { get; set; }
        public bool IsFreePlan { get; set; }
    }
}
