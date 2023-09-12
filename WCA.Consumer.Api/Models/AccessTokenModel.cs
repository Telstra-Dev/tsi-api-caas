using Newtonsoft.Json;

namespace WCA.Consumer.Api.Models
{
    public class AccessTokenModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set;}
    }
}
