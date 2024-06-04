using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace WCA.Consumer.Api.Models.UMS
{
    public partial class UmsOperationModels
    {
        public class UmsAddUserRequest
        {
            [JsonPropertyName("productVerticalClientId")]
            public string ProductVerticalClientId { get; set; }

            [JsonPropertyName("userName")]
            public string UserName { get; set; }

            [JsonPropertyName("givenName")]
            public string GivenName { get; set; }

            [JsonPropertyName("familyName")]
            public string FamilyName { get; set; }

            [JsonPropertyName("roles")]
            public List<UmsUserRole> Roles { get; set; }
        }

        public class UmsUpdateUserRequest
        {
            [JsonPropertyName("actions")]
            public string Actions { get; set; }

            [JsonPropertyName("roles")]
            public List<UmsUserRole> Roles { get; set; }
        }

        public class UmsUsers
        {
            [JsonPropertyName("data")]
            public UmsUser[] Data { get; set; }
        }

        public class UmsUser
        {
            [JsonPropertyName("userName")]
            public string UserName { get; set; }

            [JsonPropertyName("roles")]
            public List<UmsUserRole> Roles { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("meta")]
            public UmsUserMetadata Meta { get; set; }

            [JsonPropertyName("active")]
            public bool Active { get; set; }

            [JsonIgnore]
            public string CIDN => Roles?.FirstOrDefault()?.Value.Split(":")[0];

            [JsonIgnore]
            public string Role => Roles?.FirstOrDefault()?.Value.Split(":")[1];
        }

        public class UmsUserRole
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        public class UmsUserMetadata
        {
            [JsonPropertyName("created")]
            public DateTime Created { get; set; }

            [JsonPropertyName("location")]
            public string Location { get; set; }
        }
    }
}
