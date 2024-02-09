using WebX.Security.Shared.Types;

namespace WebX.Security.Shared.Behaviours
{
    public interface IClaimsFactory
    {
        JwtClaimContext GetJWTContext();
    }
}
