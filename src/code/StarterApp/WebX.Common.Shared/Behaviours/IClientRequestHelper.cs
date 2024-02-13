using System.Collections.Generic;
using WebX.Common.Shared.Types;

namespace WebX.Common.Shared.Behaviours
{
    public interface IClientRequestHelper
    {
        ClientRequestM BuildClientRequest(Dictionary<string, string> parameters);
    }
}
