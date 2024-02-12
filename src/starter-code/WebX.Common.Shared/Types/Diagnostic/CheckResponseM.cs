using System.Collections.Generic;

namespace WebX.Common.Shared.Types.Diagnostic
{
    public class CheckResponseM
    {
        public string IpAddress { get; set; } = default!;
        public Dictionary<string, bool> Checklist { get; set; } = default!;
    }
}
