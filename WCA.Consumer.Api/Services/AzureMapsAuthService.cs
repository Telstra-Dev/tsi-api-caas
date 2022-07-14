using Telstra.Core.Data.Entities;
using Telstra.Core.Contracts;

namespace WCA.Consumer.Api.Services
{
    public class AzureMapsAuthService : IAzureMapsAuthService
    {
        public AzureMapsAuthService()
        {
        }

        public AuthToken GetAuthToken(string clientId, string clientSecret)
        {
            AuthToken token = new AuthToken();

            return token;
        }
    }
}
