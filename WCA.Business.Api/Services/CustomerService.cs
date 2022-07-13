using System;
using System.Threading.Tasks;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;
using Telstra.Core.Repo;
using Grpc.Net.Client;
using System.Net;
using System.Net.Http;
using Telstra.Core.Api.Helpers;
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
        public async Task<Telstra.Core.Data.Entities.Customer> GetCustomerById(int id)
        {
            HttpClient.DefaultProxy = new WebProxy();
            var reply = await _client.GetCustomerById2Async(
                new CustomerModelRequest { CustomerId = id });
            Telstra.Core.Data.Entities.Customer _customer = new Telstra.Core.Data.Entities.Customer();
            _customer.CustomerId = reply.CustomerId;
            _customer.Name = reply.Name;
            _customer.Alias = reply.Alias;
            return _customer;
        }
    }
}
