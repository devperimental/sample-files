using PlatformX.ServiceLayer.Common.Behaviours;

namespace PlatformX.ServiceLayer.Common.Types
{
    public class GetRequestM : IGetRequestM
    {
        public string? GlobalId { get; set; }
    }
}
