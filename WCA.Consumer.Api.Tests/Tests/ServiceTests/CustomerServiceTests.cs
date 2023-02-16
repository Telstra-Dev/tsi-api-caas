using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Models.StorageReponse;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;
using Xunit;

namespace WCA.Customer.Api.Tests
{ 
    public class CustomerServiceTests
    {
        [Fact]
        public void GetCustomerById2()
        {
            var customerId = 1;
            var myCustomer = TestDataHelper.CreateCustomer();
            var customerResponse = new CustomerResponse();
            customerResponse.Result = new List<WCA.Consumer.Api.Models.Customer>()
            {
                myCustomer
            };

            var appSettings = TestDataHelper.CreateAppSettings();
            
            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(customerResponse);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/customer/{myCustomer.CustomerId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            CustomerService customerService = new CustomerService(null, httpClientMock, appSettings);

            var result = customerService.GetCustomerById2(customerId).Result;

            Assert.Equal(typeof(WCA.Consumer.Api.Models.Customer), result.GetType());
            Assert.Equal(result.CustomerId, myCustomer?.CustomerId);
        }
    }
}
