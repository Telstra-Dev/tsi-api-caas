using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IRestClient
    {
        Task<T> GetAsync<T>(string uriString, CancellationToken cancellationToken);
        Task<T> PostAsync<T>(string uriString, string payload, CancellationToken cancellationToken);
        Task<T> PutAsync<T>(string uriString, string payload, CancellationToken cancellationToken);
        Task<T> DeleteAsync<T>(string uriString, CancellationToken cancellationToken);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<HttpResponseMessage> SendWithResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken);
        Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
        void AddBearerToken(string token);
    }
}
