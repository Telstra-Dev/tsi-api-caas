using System.Threading.Tasks;
using Telstra.Core.Data.Entities.AzureMapsResponse;

namespace Telstra.Core.Contracts
{
    public interface IAzureMapsAuthService
    {
        public Task<AuthToken> GetAuthToken(string uri, string clientId, string clientSecret, string grantType, string resource);
    }
}
