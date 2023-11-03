// using System.Threading.Tasks;
// using WCA.Consumer.Api.Models.StorageReponse;
// using WCA.Consumer.Api.Services.Contracts;
// using System.Net.Http;
// using Telstra.Common;
// using WCA.Storage.Api.Proto;
// using Newtonsoft.Json;
// using System.Threading;

// namespace WCA.Consumer.Api.Services
// {
//     public class CustomerService : ICustomerService
//     {
//         private readonly WCA.Storage.Api.Proto.Customer.CustomerClient _grpcClient;
//         private readonly IRestClient _httpClient;
//         private readonly AppSettings _appSettings;

//         public CustomerService(WCA.Storage.Api.Proto.Customer.CustomerClient grpcClient, IRestClient httpClient, AppSettings appSettings)
//         {
//             _grpcClient = grpcClient;
//             _httpClient = httpClient;
//             _appSettings = appSettings;
//         }
//         public async Task<WCA.Consumer.Api.Models.Customer> GetCustomerById(int id)
//         {
//             // Remove for Development under VPN
//             //HttpClient.DefaultProxy = new WebProxy();
//             var reply = await _grpcClient.GetCustomerById2Async(
//                 new CustomerModelRequest { CustomerId = id });
//             WCA.Consumer.Api.Models.Customer _customer = new WCA.Consumer.Api.Models.Customer();
//             _customer.CustomerId = reply.CustomerId;
//             _customer.Name = reply.Name;
//             _customer.Alias = reply.Alias;
//             return _customer;
//         }

//         public async Task<WCA.Consumer.Api.Models.Customer> GetCustomerById2(int id)
//         {
//             var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/customer/{id}");
//             var customerResponse = await _httpClient.SendAsync<CustomerResponse>(request, CancellationToken.None);

//             return customerResponse.Result[0];
//         }
//     }
// }
