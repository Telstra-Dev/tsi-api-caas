using System.Threading.Tasks;
using WCA.Consumer.Api.Models.StorageReponse;
using WCA.Consumer.Api.Services.Contracts;
using System.Net;
using System.Net.Http;
using Telstra.Common;
using WCA.Storage.Api.Proto;
using Newtonsoft.Json;

namespace WCA.Consumer.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly WCA.Storage.Api.Proto.Customer.CustomerClient _grpcClient;
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;

        public CustomerService(WCA.Storage.Api.Proto.Customer.CustomerClient grpcClient, HttpClient httpClient, AppSettings appSettings)
        {
            this._grpcClient = grpcClient;
            this._httpClient = httpClient;
            this._appSettings = appSettings;
        }
        public async Task<WCA.Consumer.Api.Models.Customer> GetCustomerById(int id)
        {
            // Remove for Development under VPN
            //HttpClient.DefaultProxy = new WebProxy();
            var reply = await _grpcClient.GetCustomerById2Async(
                new CustomerModelRequest { CustomerId = id });
            WCA.Consumer.Api.Models.Customer _customer = new WCA.Consumer.Api.Models.Customer();
            _customer.CustomerId = reply.CustomerId;
            _customer.Name = reply.Name;
            _customer.Alias = reply.Alias;
            return _customer;
        }

        public async Task<WCA.Consumer.Api.Models.Customer> GetCustomerById2(int id)
        {
            var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/customer/{id}");
            var reply = await response.Content.ReadAsStringAsync();
            CustomerResponse customerResponse = JsonConvert.DeserializeObject<CustomerResponse>(reply);
            return customerResponse.Result[0];
        }
    }
}
