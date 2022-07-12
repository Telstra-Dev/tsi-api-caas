using System.Threading.Tasks;
using WCA.Business.Api.Services.ServicesInterfaces;
using System.Net;
using System.Net.Http;
using WCA.Business.Api.Models;
using WCA.Storage.Api.Proto;

namespace WCA.Business.Api.Services
{
    public class CustomerService : ICustomerService
    {
       private readonly WCA.Storage.Api.Proto.Customer.CustomerClient _client;
        public CustomerService(WCA.Storage.Api.Proto.Customer.CustomerClient client)
        {
            this._client = client;
        }
        public async Task<WCA.Business.Api.Models.Customer> GetCustomerById(int id)
        {
            HttpClient.DefaultProxy = new WebProxy();
            var reply = await _client.GetCustomerById2Async(
                new CustomerModelRequest { CustomerId = id });
            WCA.Business.Api.Models.Customer _customer = new WCA.Business.Api.Models.Customer();
            _customer.CustomerId = reply.CustomerId;
            _customer.Name = reply.Name;
            _customer.Alias = reply.Alias;
            return _customer;
        }
    }
}
