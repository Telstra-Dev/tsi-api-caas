using Flurl;

namespace Telstra.Common.Models
{
    public class AzureAdConfiguration
    {
        public string ClientId { get; set; }
        public string Instance { get; set; }
        public string TenantId { get; set; }
        public string KeyInApplicationSettings { get; set; }

        //$@"{this.Instance}/{this.TenantId}/{this.Policy}/v2.0/";
        public string Authority => Url.Combine(this.Instance, this.TenantId, this.Policy, "v2.0");

        //$@"{this.Authority}.well-known/openid-configuration?p={this.Policy}";
        public string OpenIdConnectConfigurationUrl => Url.Combine(this.Authority, ".well-known/openid-configuration")
            .SetQueryParam("p", this.Policy);

        public string Policy { get; set; }

        //$@"{this.Instance}/{this.TenantId}/{this.Policy}/oauth2/v2.0/authorize?p={this.Policy}";
        public string AuthorizationUrl =>
            Url.Combine(this.Instance, this.TenantId, this.Policy, "/oauth2/v2.0/authorize")
                .SetQueryParam("p", this.Policy);

        //$@"{this.Instance}/{this.TenantId}/{this.Policy}/oauth2/v2.0/token?p={this.Policy}";
        public string TokenUrl => Url.Combine(this.Instance, this.TenantId, this.Policy, "oauth2/v2.0/token")
            .SetQueryParam("p", this.Policy);

        //$@"https://{this.TenantId}/{this.ClientId}/app.read";
        public string DefaultScope => Url.Combine($@"https://{this.TenantId}", this.ClientId, "app.read");

        
    }
}
