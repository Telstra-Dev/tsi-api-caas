using System;
using System.Collections.Generic;
using Telstra.Common.Models;

namespace Telstra.Common
{
    public class StorageSettings
    {
        public DBSetting MyDb { get; set; }
        public DBSetting TenantDb { get; set; }
    }
    public class AppSettings
    {
        public string[] StaticPaths { get; set; }
        public StorageSettings Storage { get; set; }
        public AzureAdConfiguration AzureAd { get; set; }
        public bool UseAd { get; set; }
    }

    public class DBSetting
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionStringTemplate { get; set; }
        public bool UseInMemory { get; set; }
        public string DatabaseName { get; set; }
        public string Schema { get; set; }

        public string ConnectionString => this.ConnectionStringTemplate
            .Replace("{{host}}", this.Host)
            .Replace("{{port}}", this.Port.ToString())
            .Replace("{{username}}",this.Username)
            .Replace("{{password}}", this.Password)
            .Replace("{{database}}", this.DatabaseName);
    }
}
