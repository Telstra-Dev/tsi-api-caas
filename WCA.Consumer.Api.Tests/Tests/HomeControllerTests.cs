using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Telstra.Core.Contracts;
using WCA.Consumer.Api.Controllers;
using Xunit;

namespace Telstra.Consumer.Api.Tests
{ 
    public class HomeControllerTests
    {
        [Fact(DisplayName = "API - Ping Test")]
        public void Ping_Test()
        {
            var expectedResult = new { result = "API up and Running" };
            var serviceMock = new Mock<IHomeService>(MockBehavior.Strict);
            var controller = new HomeController(serviceMock.Object);

            var result = controller.Get();

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);

            var actualResult = (result as OkObjectResult).Value as object;
            Assert.Equal(JsonConvert.SerializeObject(expectedResult), JsonConvert.SerializeObject(actualResult));
        }
    }
}
