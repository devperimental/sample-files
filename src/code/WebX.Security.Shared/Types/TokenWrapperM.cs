using System.Text.Json.Serialization;

namespace WebX.Security.Shared.Types
{
    public class TokenWrapperM
    {
        [JsonIgnore(Condition= JsonIgnoreCondition.WhenWritingDefault)]
        public AuthStateM? AuthState { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] 
        public BearerTokenM? BearerToken { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] 
        public string? AdditionalData { get; set; }
    }
}
