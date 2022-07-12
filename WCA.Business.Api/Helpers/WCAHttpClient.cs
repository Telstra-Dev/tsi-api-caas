using System;
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
        private StorageFunctionAppConfig config;
        private static bool _isDevelopment = false;
        private static HttpClient httpClient;

        private static string STORAGE_FUNCTION_APP_CONFIG = "StorageFunctionApp";

        SemaphoreSlim mutex = new SemaphoreSlim(1);

        public WCAHttpClient()
        {
            if (Program.ConfigurationBuilder.GetSection(STORAGE_FUNCTION_APP_CONFIG).Exists())
            {
                config = Program.ConfigurationBuilder.GetSection(STORAGE_FUNCTION_APP_CONFIG)
                                        .Get<StorageFunctionAppConfig>();
            }
            
            if (isDevelopment() && httpClient == null)
            {
                if ((config != null) && (config.baseUrl != null))
                {
                    httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri(config.baseUrl);             
                }
                else 
                {
                    throw new Exception("StorageFunctionAppConfig not found in configuration (appsettings.json).");
                }       
            }
            else if (httpClient == null)
            {
                if ((config != null) && (config.baseUrl != null))
                {
                    var handler = new HttpClientHandler();
                    httpClient = new HttpClient(handler);
                    httpClient.BaseAddress = new Uri(config.baseUrl);  
                }      
                else 
                {
                    throw new Exception("StorageFunctionAppConfig not found in configuration.");
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

        public async Task<string> CallFunctionApp()
        {
            string content = String.Empty;

            HttpResponseMessage response = new HttpResponseMessage();  

            await mutex.WaitAsync();
            try 
            {
                response = httpClient.GetAsync(config.code).Result;
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
    
    public class StorageFunctionAppConfig 
    {
        public string baseUrl  { get; set; }

        public string code  { get; set; }
    }
}