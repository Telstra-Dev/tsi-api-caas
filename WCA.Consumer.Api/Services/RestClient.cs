using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Services
{
    internal class RestClient : IRestClient
    {
        private HttpClient _httpClient;
        private ILogger<RestClient> _logger;

        public RestClient(IHttpClientFactory clientFactory, ILogger<RestClient> logger)
        {
            _httpClient = clientFactory.CreateClient(nameof(CustomerService));
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void AddBearerToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        public async Task<T> GetAsync<T>(string uriString, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uriString);
            var response = await SendAsync<T>(request, cancellationToken);

            return response;
        }

        public async Task<T> PutAsync<T>(string uriString, string payload, CancellationToken cancellationToken)
        {
            HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, uriString);
            request.Content = httpContent;
            var response = await SendAsync<T>(request, cancellationToken);

            return response;
        }

        public async Task<T> PostAsync<T>(string uriString, string payload, CancellationToken cancellationToken)
        {
            HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uriString);
            request.Content = httpContent;
            var response = await SendAsync<T>(request, cancellationToken);

            return response;
        }

        public async Task<T> DeleteAsync<T>(string uriString, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uriString);
            var response = await SendAsync<T>(request, cancellationToken);

            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {

                    var contentString = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(contentString);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error calling api {@statusCode} {method} {requestUri} {@error}", response?.StatusCode, request.Method, request.RequestUri, ex.Message);

                throw new HttpRequestException(ex.Message);
            }
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await this.SendAsync(request, cancellationToken);
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var reply = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(reply, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fail to convert http response data into {typeof(T)}");

                throw new Exception(ex.Message);
            }
        }
    }
}
