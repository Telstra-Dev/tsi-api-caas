using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities.AzureMapsResponse;
using Telstra.Core.Contracts;
using Newtonsoft.Json;

namespace WCA.Consumer.Api.Services
{
    public class AzureMapsAuthService : IAzureMapsAuthService
    {
        private readonly HttpClient _httpClient;

        public AzureMapsAuthService(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<AuthToken> GetAuthToken(
            string uri, string clientId, string clientSecret, string grantType, string resource)
        {
            AuthToken authToken = new AuthToken();
            
            _httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); 

            var data = new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", grantType),
                new KeyValuePair<string, string>("resource", resource)
            };  
            
            var response = _httpClient.PostAsync(
                $"{uri}", 
                new FormUrlEncodedContent(data)).Result;
                
            var reply = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)  
            {  
                authToken = JsonConvert.DeserializeObject<AuthToken>(reply);
            }
            else
            {
                throw new Exception("Error calling Azure Maps Api. Response code: " + response.StatusCode); 
            }

            return authToken;
        }
    }
}
