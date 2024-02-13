using System.Collections.Generic;
using WebX.Security.Shared.EnumTypes;
using WebX.Security.Shared.Types;

namespace WebX.Security.Shared.Behaviours
{
    public interface ITokenHelper
    {
        TokenWrapperM CreateBearerToken(TokenContextData tokenContext, VerifyActionType actionType);
        TokenWrapperM CreateScopedBearerToken(TokenContextData tokenContext, string scope);
        BearerTokenM CreateToken(TokenContextData tokenContext, Dictionary<string, string> itemsForClaim, TokenType tokenType);
        UserDeviceHashM CreateUserDeviceHash(string identityId);
        bool ValidateUserDeviceHash(string hash, string identityId);
    }
}
