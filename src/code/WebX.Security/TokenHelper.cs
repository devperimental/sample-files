using DomainX.Identity.Shared.DataContract;
using DomainX.Portal.Shared.Behaviours;
using Microsoft.IdentityModel.Tokens;
using PlatformX.FileStore.Shared.Behaviours;
using PlatformX.Messaging.Types;
using PlatformX.Messaging.Types.Constants;
using PlatformX.Utility.Shared.Behaviours;
using PlatformX.Utility.Shared.EnumTypes;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using WebX.Security.Shared.Behaviours;
using WebX.Security.Shared.Constants;
using WebX.Security.Shared.EnumTypes;
using WebX.Security.Shared.Types;

namespace WebX.Security
{
    public class TokenHelper : ITokenHelper
    {
        private readonly RequestContext _requestContext;
        private readonly IFileStore _fileStore;
        private readonly IHashGenerator _hashGenerator;
        private readonly ITokenSettings _tokenSettings;
        public TokenHelper(RequestContext requestContext,
                    IFileStore fileStore,
                    IHashGenerator hashGenerator,
                    ITokenSettings tokenSettings)
        {
            _requestContext = requestContext;
            _fileStore = fileStore;
            _hashGenerator = hashGenerator;
            _tokenSettings = tokenSettings;
        }

        public TokenWrapperM CreateBearerToken(TokenContextData tokenContext, VerifyActionType actionType)
        {
            AuthStateM authState;

            BearerTokenM bearerToken;
            switch (_requestContext.SourceTypeKey)
            {
                case SystemApiRoleType.Management:
                {
                    authState = GetPortalSignupState(tokenContext);

                    switch (actionType)
                    {
                        case VerifyActionType.SIGNIN:
                        case VerifyActionType.SIGNUP:
                            if (authState.SignupScope == SignUpScope.FULL)
                            {
                                authState.MFAChoices = tokenContext.MFAChoices;
                                authState.SigninScope = tokenContext.MFAEnabled ? SignInScopes.REQUIREMFA : SignInScopes.COMPLETE;
                            }
                            else
                            {
                                authState.SigninScope = SignInScopes.SIGNUPINCOMPLETE;
                            }
                            break;
                            //case VerifyActionType.VERIFY:
                            //   authState.SigninScope = SignInScopes.COMPLETE;
                            //   break;
                    };

                    bearerToken = CreateManagementBearerToken(authState, tokenContext);
                    break;
                }
                case SystemApiRoleType.Client:
                {
                    authState = GetClientSignupState(tokenContext, tokenContext.SignupRequirement);
                    authState.MFAChoices = tokenContext.MFAChoices;
                    var vMFAEnabled = tokenContext.MFAEnabled;

                    switch (actionType)
                    {
                        case VerifyActionType.SIGNIN:
                        case VerifyActionType.SIGNUP:
                            if (authState.SignupScope == SignUpScope.FULL)
                            {
                                authState.MFAChoices = tokenContext.MFAChoices;
                                authState.SigninScope = tokenContext.MFAEnabled ? SignInScopes.REQUIREMFA : SignInScopes.COMPLETE;
                            }
                            else
                            {
                                authState.SigninScope = SignInScopes.SIGNUPINCOMPLETE;
                            }
                            break;
                            //case VerifyActionType.VERIFY:
                            //   authState.SigninScope = SignInScopes.COMPLETE;
                            //   break;
                    };

                    bearerToken = CreateClientBearerToken(tokenContext, authState);
                    break;
                }
                default:
                    throw new ApplicationException($"Invalid SystemApiRoleType {_requestContext.SourceTypeKey} in CreateBearerToken");
            }

            return new TokenWrapperM
            {
                AuthState = authState,
                BearerToken = bearerToken
            };
        }

        public TokenWrapperM CreateScopedBearerToken(TokenContextData tokenContext, string scope)
        {
            Dictionary<string, string> itemsForClaim = new Dictionary<string, string>()
            {
                { APIClaim.IdentityId, tokenContext.IdentityId! },
                { APIClaim.SessionId, tokenContext.SessionId! },
                { APIClaim.CustomScope, scope }
            };

            if (_requestContext.SourceTypeKey == SystemApiRoleType.Management)
            {
                itemsForClaim.Add(APIClaim.PortalName, tokenContext.PortalName!);
            }
            else if (_requestContext.SourceTypeKey == SystemApiRoleType.Client)
            {
                itemsForClaim.Add(APIClaim.ClientApplicationKey, _requestContext.ClientApplicationKey!);
            }

            return new TokenWrapperM
            {
                BearerToken = CreateToken(tokenContext, itemsForClaim, TokenType.Bearer)
            };
        }

        public BearerTokenM CreateToken(TokenContextData tokenContext, Dictionary<string, string> itemsForClaim, TokenType tokenType)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.TokenSecurityKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature, SecurityAlgorithms.Sha512Digest);

            var claimList = itemsForClaim.Select(item => new Claim(item.Key, item.Value)).ToList();

            var claimsIdentity = new ClaimsIdentity(claimList, "Custom");

            var tokenExpiry = DateTime.UtcNow.AddMinutes(_tokenSettings.TokenExpiryMinutes);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _tokenSettings.TokenAudience,
                Issuer = _tokenSettings.TokenIssuer,
                Subject = claimsIdentity,
                SigningCredentials = signingCredentials,
                Expires = tokenExpiry
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            var signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

            return new BearerTokenM { TokenValue = signedAndEncodedToken, TokenExpiryUTC = tokenExpiry };
        }

        private AuthStateM GetPortalSignupState(TokenContextData tokenContext)
        {
            var authState = new AuthStateM();

            var signUpState = GetSignUpState(tokenContext);
            var signUpRequirement = tokenContext.SignupRequirement;

            string signUpScope;

            if (signUpState < signUpRequirement)
            {
                signUpScope = signUpState < SignupFlag.EMAIL ? SignUpScope.SIGNUP : SignUpScope.MFA_SETUP;
            }
            else
            {
                signUpScope = GetSignUpScope(tokenContext);
            }

            authState.SignupRequirement = signUpRequirement;
            authState.SignupState = signUpState;
            authState.SignupScope = signUpScope;

            return authState;
        }

        private int GetSignUpState(TokenContextData tokenContext)
        {
            var signupState = 0;

            if (tokenContext.EmailConfirmed)
            {
                signupState += SignupFlag.EMAIL;
            }

            if (tokenContext.PhoneNumberConfirmed)
            {
                signupState += SignupFlag.MOBILE;
            }

            if (tokenContext.AlternateEmailConfirmed)
            {
                signupState += SignupFlag.ALTERNATEEMAIL;
            }

            return signupState;
        }

        private string GetSignUpScope(TokenContextData tokenContext)
        {
            var scope = SignUpScope.ONBOARDING;

            if (!string.IsNullOrEmpty(tokenContext.UserGlobalId) &&
                !string.IsNullOrEmpty(tokenContext.OrganisationGlobalId))
            {
                scope = SignUpScope.ONBOARDING_PLAN;
            }

            if (!string.IsNullOrEmpty(tokenContext.PlanGlobalId))
            {
                scope = tokenContext.IsFreePlan ? SignUpScope.FULL : SignUpScope.ONBOARDING_PAYMENT;
            }

            if (!string.IsNullOrEmpty(tokenContext.ActiveSubscription))
            {
                scope = SignUpScope.FULL;
            }

            return scope;
        }

        //private PortalContextM LoadPortalInfo(string identityId)
        //{
        //    var userInformationGlobalId = string.Empty;
        //    var userInfoResponse = _portalProvider.GetUserInformationByIdentityId(new UserInformation
        //    {
        //        IdentityId = identityId
        //    }, _requestContext);

        //    if (userInfoResponse.Success)
        //    {
        //        userInformationGlobalId = userInfoResponse.UserInfo?.GlobalId;
        //    }

        //    var organisationGlobalId = string.Empty;
        //    if (!string.IsNullOrEmpty(userInformationGlobalId))
        //    {
        //        var organisationResponse = _portalProvider.GetOrganisationByUserGlobalId(new Organisation
        //        {
        //            ActionUserGlobalId = userInformationGlobalId
        //        }, _requestContext);

        //        if (organisationResponse.Success)
        //        {
        //            organisationGlobalId = organisationResponse.Organisation?.GlobalId;
        //        }
        //    }

        //    return new PortalContextM { UserGlobalId = userInformationGlobalId, OrganisationGlobalId = organisationGlobalId };
        //}

        private BearerTokenM CreateManagementBearerToken(AuthStateM signupState,
            TokenContextData tokenContext)
        {
            var itemsForClaim = new Dictionary<string, string>
            {
                { APIClaim.IdentityId, tokenContext.IdentityId! },
                { APIClaim.PortalName, tokenContext.PortalName! },
                { APIClaim.SessionId, tokenContext.SessionId! },
                { APIClaim.SignupRequirement, signupState.SignupRequirement.ToString() },
                { APIClaim.SignupState, signupState.SignupState.ToString() }
            };

            if (!string.IsNullOrEmpty(signupState.SignupScope))
            {
                itemsForClaim.Add(APIClaim.SignupScope, signupState.SignupScope);
            }

            if (!string.IsNullOrEmpty(signupState.SigninScope))
            {
                itemsForClaim.Add(APIClaim.SigninScope, signupState.SigninScope);
            }

            if (!string.IsNullOrEmpty(tokenContext.UserGlobalId))
            {
                itemsForClaim.Add(APIClaim.UserGlobalId, tokenContext.UserGlobalId);
            }

            if (!string.IsNullOrEmpty(tokenContext.OrganisationGlobalId))
            {
                itemsForClaim.Add(APIClaim.OrganisationGlobalId, tokenContext.OrganisationGlobalId);
            }

            var bearerToken = CreateToken(tokenContext, itemsForClaim, TokenType.Bearer);

            return bearerToken;
        }

        private BearerTokenM CreateClientBearerToken(TokenContextData tokenContext,
            AuthStateM signupState)
        {
            var itemsForClaim = new Dictionary<string, string>
            {
                { APIClaim.IdentityId, tokenContext.IdentityId! },
                { APIClaim.ClientApplicationKey, tokenContext.ClientApplicationKey! },
                { APIClaim.SessionId, tokenContext.SessionId! },
                { APIClaim.SignupRequirement, signupState.SignupRequirement.ToString() },
                { APIClaim.SignupState, signupState.SignupState.ToString() }
            };

            if (!string.IsNullOrEmpty(signupState.SignupScope))
            {
                itemsForClaim.Add(APIClaim.SignupScope, signupState.SignupScope);
            }

            if (!string.IsNullOrEmpty(signupState.SigninScope))
            {
                itemsForClaim.Add(APIClaim.SigninScope, signupState.SigninScope);
            }

            var bearerToken = CreateToken(tokenContext, itemsForClaim, TokenType.Bearer);

            return bearerToken;
        }

        private AuthStateM GetClientSignupState(TokenContextData tokenContext, int signupRequirement)
        {
            var authState = new AuthStateM();

            var signupState = GetSignUpState(tokenContext);

            string signupScope;

            if (signupState < signupRequirement)
            {
                signupScope = signupState == 0 ? SignUpScope.SIGNUP : SignUpScope.MFA_SETUP;
            }
            else
            {
                signupScope = SignUpScope.FULL;
            }

            authState.SignupRequirement = signupRequirement;
            authState.SignupState = signupState;
            authState.SignupScope = signupScope;

            return authState;
        }

        public UserDeviceHashM CreateUserDeviceHash(string identityId)
        {
            var validUntil = DateTime.UtcNow.AddDays(30);
            var stringToHash = $"{_requestContext.UserAgent}|{identityId}";

            var userDeviceHashM = new UserDeviceHashM
            {
                Hash = _hashGenerator.CreateHash(stringToHash, HashType.SHA512),
                ValidUntil = validUntil
            };

            var userDeviceHash = new UserDeviceHash
            {
                IdentityId = _requestContext.IdentityId,
                DeviceHash = userDeviceHashM.Hash,
                ValidUntil = userDeviceHashM.ValidUntil
            };

            //_identityServiceClient.SaveUserDeviceHash(userDeviceHash, _requestContext);

            return userDeviceHashM;
        }

        public bool ValidateUserDeviceHash(string hash, string identityId)
        {
            var stringToHash = $"{_requestContext.UserAgent}|{identityId}";
            var compareHash = _hashGenerator.CreateHash(stringToHash, HashType.SHA512);

            if (hash != compareHash)
            {
                return false;
            }

            var userDeviceHash = new UserDeviceHash
            {
                IdentityId = identityId,
                DeviceHash = hash
            };

            //var response = _identityServiceClient.GetUserDeviceHash(userDeviceHash, _requestContext);

            //if (response.Success)
            //{
            //    return response.UserDeviceHash?.ValidUntil > DateTime.UtcNow;
            //}

            return false;

        }
    }
}