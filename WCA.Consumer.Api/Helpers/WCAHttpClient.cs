using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Threading;

namespace Telstra.Core.Api.Helpers
{
    public class WCAHttpClient
    {
        private static WebProxy proxy = new WebProxy 
                                 {
                                    Address = new Uri($"http://localhost:8080"),
                                    BypassProxyOnLocal = false,
                                    UseDefaultCredentials = false
                                };
        private AzureMapsAppConfig config;
        private static bool _isDevelopment = false;
        private static HttpClient httpClient;

        private static string AZURE_MAPS_APP_CONFIG = "AzureMapsAuthCredentials";

        SemaphoreSlim mutex = new SemaphoreSlim(1);

        public WCAHttpClient()
        {
            if (Program.ConfigurationBuilder.GetSection(AZURE_MAPS_APP_CONFIG).Exists())
            {
                config = Program.ConfigurationBuilder.GetSection(AZURE_MAPS_APP_CONFIG)
                                        .Get<AzureMapsAppConfig>();
            }
            
            if (isDevelopment() && httpClient == null)
            {
                if ((config != null) && (config.AuthUri != null))
                {
                    httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri(config.AuthUri);             
                }
                else 
                {
                    throw new Exception("AzureMapsAppConfig not found in configuration (appsettings.json).");
                }       
            }
            else if (httpClient == null)
            {
                if ((config != null) && (config.AuthUri != null))
                {
                    var handler = new HttpClientHandler();
                    httpClient = new HttpClient(handler);
                    httpClient.BaseAddress = new Uri(config.AuthUri);  
                }      
                else 
                {
                    throw new Exception("AzureMapsAppConfig not found in configuration.");
                }          
            } 
        }

        public WCAHttpClient(HttpMessageHandler handler, string baseUrl = null) : this()
        {
            httpClient = new HttpClient(handler);
            if (baseUrl != null)
            {
                httpClient.BaseAddress = new Uri(baseUrl);    
                httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));   
            }         
        }

        public async Task<string> GetAzureMapsAuthToken()
        {
            string content = String.Empty;

            HttpResponseMessage response = new HttpResponseMessage();  

            var data = new[]
            {
                new KeyValuePair<string, string>("client_id", config.ClientId),
                new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                new KeyValuePair<string, string>("grant_type", config.GrantType),
                new KeyValuePair<string, string>("resource", config.Resource),
            };  

            await mutex.WaitAsync();
            try 
            {
                response = httpClient.PostAsync(config.AuthUri, new FormUrlEncodedContent(data)).Result;
                httpClient.DefaultRequestHeaders.Clear();
            }
            finally 
            {
                mutex.Release();
            }

            if (response.IsSuccessStatusCode)  
            {  
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Error calling storage function app. Response code: " + response.StatusCode); 
            }
            
            return content;
        }

        
        private static bool isDevelopment()
        {
            // Assume prod but override if dev
            if (!_isDevelopment)
            {
                var _newValue = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (_newValue != null && _newValue == "Development")
                    _isDevelopment = true;
            }
            
            return _isDevelopment;
        }

    }
    
    public class AzureMapsAppConfig 
    {
        public string AuthUri  { get; set; }
        public string ClientId  { get; set; }
        public string ClientSecret  { get; set; }
        public string GrantType  { get; set; }
        public string Resource  { get; set; }
    }
}