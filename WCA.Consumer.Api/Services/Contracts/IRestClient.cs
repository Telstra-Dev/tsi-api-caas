using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IRestClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
        void AddBearerToken(string token);
    }
}
