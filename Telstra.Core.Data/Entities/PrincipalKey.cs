using System;
using Telstra.Common;

namespace Telstra.Core.Data.Entities
{
    public class PrincipalKey
    {
        public const string Name = "Name";
        public const string Email = "Email";
        public const string ObjectId = "OID";

        public PrincipalKey(string key, string type)
        {
            Key = key;
            Type = type;
        }

        public string Key { get; set; }
        public string Type { get; set; }
        public string HashedKey => this.Key.ToSha256Hash();
    }
}
