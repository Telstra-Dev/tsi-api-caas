using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Controllers;
using Xunit;

namespace WCA.Customer.Api.Tests
{ 
    public class CustomerControllerTests
    {
        [Fact(DisplayName = "Get one customer")]
        public void GetCustomerById()
        {
            var serviceMock = new Mock<ICustomerService>(MockBehavior.Strict);
            var customerId = 1;
            var myCustomer = TestDataHelper.CreateCustomer();
            serviceMock.Setup(m => m.GetCustomerById(It.IsAny<int>())).Returns(Task.FromResult(myCustomer));

            var controller = new CustomerController(serviceMock.Object);

            var result = controller.GetCustomerById(customerId).Result;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int) HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            WCA.Consumer.Api.Models.Customer expectedCustomer = ((result as OkObjectResult).Value as WCA.Consumer.Api.Models.Customer);
            Assert.Equal(expectedCustomer.CustomerId, myCustomer?.CustomerId);
        }
    }
}
