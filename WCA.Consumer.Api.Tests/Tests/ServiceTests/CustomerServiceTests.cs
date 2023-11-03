// using System.Collections.Generic;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using Moq;
// using WCA.Consumer.Api.Models.StorageReponse;
// using WCA.Consumer.Api.Services;
// using WCA.Consumer.Api.Services.Contracts;
// using Xunit;

// namespace WCA.Customer.Api.Tests
// {
//     public class CustomerServiceTests
//     {
//         [Fact]
//         public async Task GetCustomerById2()
//         {
//             var customerId = 1;
//             var myCustomer = TestDataHelper.CreateCustomer();
//             var customerResponse = new CustomerResponse();
//             customerResponse.Result = new List<WCA.Consumer.Api.Models.Customer>()
//             {
//                 myCustomer
//             };

//             var appSettings = TestDataHelper.CreateAppSettings();

//             var httpClientMock = new Mock<IRestClient>();
//             httpClientMock.Setup(x => x.SendAsync<CustomerResponse>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(customerResponse);

//             CustomerService customerService = new CustomerService(null, httpClientMock.Object, appSettings);

//             var result = await customerService.GetCustomerById2(customerId);

//             Assert.Equal(typeof(WCA.Consumer.Api.Models.Customer), result.GetType());
//             Assert.Equal(result.CustomerId, myCustomer?.CustomerId);
//         }
//     }
// }
