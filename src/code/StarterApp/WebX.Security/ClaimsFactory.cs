using Microsoft.AspNetCore.Http;
using System.Linq;
using WebX.Security.Shared.Behaviours;
using WebX.Security.Shared.Constants;
using WebX.Security.Shared.Types;

namespace WebX.Security
{
    public class ClaimsFactory : IClaimsFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public JwtClaimContext GetJWTContext()
        {
            if (!_httpContextAccessor.HttpContext.User.Claims.Any())
            {
                return new JwtClaimContext();
            }

            var portalName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.PortalName)?.Value;
            var identityId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.IdentityId)?.Value;
            var organisationGlobalId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.OrganisationGlobalId)?.Value;
            var userGlobalId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.UserGlobalId)?.Value;
            var sessionId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.SessionId)?.Value;
            var signupRequirement = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.SignupRequirement)?.Value;
            var signupState = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.SignupState)?.Value;
            var signupScope = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == APIClaim.SignupScope)?.Value;

            return new JwtClaimContext
            {
                PortalName = portalName,
                OrganisationGlobalId = organisationGlobalId,
                UserGlobalId = userGlobalId,
                IdentityId = identityId,
                SessionId = sessionId,
                SignupRequirement = signupRequirement,
                SignupState = signupState,
                SignupScope = signupScope
            };
        }
    }
}
