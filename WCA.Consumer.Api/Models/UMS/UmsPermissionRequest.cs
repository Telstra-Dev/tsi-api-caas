using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WCA.Consumer.Api.Models.UMS
{
    public partial class UmsOperationModels
    {
        public class UmsRole
        {
            [JsonPropertyName("roleType")]
            public string RoleType { get; set; }

            [JsonPropertyName("roleName")]
            public string RoleName { get; set; }
        }

        public class UmsPermissionRequest
        {
            [JsonPropertyName("role_names")]
            public UmsRole[] RoleNames { get; set; }
        }

        public class UmsPermissionResponse
        {
            [JsonPropertyName("data")]
            public List<PermissionData> Data { get; set; }
        }

        public class PermissionData
        {
            [JsonPropertyName("roleType")]
            public string RoleType { get; set; }

            [JsonPropertyName("roleName")]
            public string RoleName { get; set; }

            [JsonPropertyName("permissions")]
            public List<string> Permissions { get; set; }
        }
    }
}
