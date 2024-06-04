using System.Text.Json.Serialization;

namespace WCA.Consumer.Api.Models.UMS
{
    public class UmsUsersResponseModel
    {
        [JsonPropertyName("data")]
        public UserData[] UserData { get; set; }
    }

    public class UserData
    {
        [JsonPropertyName("userName")]
        public string Email { get; set; }

        [JsonPropertyName("roles")]
        public Role[] Roles { get; set; }

        [JsonPropertyName("active")]
        public bool IsActive { get; set; }
    }

    public class Role
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

}
