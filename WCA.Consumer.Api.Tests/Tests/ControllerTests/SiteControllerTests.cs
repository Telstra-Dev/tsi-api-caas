using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Controllers;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WCA.Customer.Api.Tests
{
    public class SiteControllerTests
    {        
        [Fact(DisplayName = "Get sites not found")]
        public async void GetSites()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var customerId = "customer-id";
            serviceMock.Setup(m => m.GetSitesForCustomer(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<IList<SiteModel>>(null));

            var httpContext = new DefaultHttpContext();
            // httpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var logger = new Mock<ILogger<SiteController>>();
            var controller = new SiteController(serviceMock.Object, logger.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await controller.GetSites("fake.user.email@example.com");

            Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundObjectResult).StatusCode);
        }
    }
}
