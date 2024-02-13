using PlatformX.Utility.Shared.EnumTypes;
using System;

namespace PlatformX.Utility.Shared.Behaviours
{
    public interface IHashGenerator
    {
        string CreateHash(string stringToHash, HashType type);
        string GenerateRequestInput(string portalName, string serviceTimestamp, string serviceSecret, string ipAddress, string correlationId);
    }
}
