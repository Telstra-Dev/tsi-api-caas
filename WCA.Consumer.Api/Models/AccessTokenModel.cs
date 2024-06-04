using System.Text.Json.Serialization;

namespace WCA.Consumer.Api.Models
{
    public class AccessTokenModel
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public string ExpiresIn { get; set;}
    }
}
