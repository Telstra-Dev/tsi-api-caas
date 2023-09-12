using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Net;
using WCA.Consumer.Api.Controllers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Customer.Api.Tests;
using Xunit;

namespace WCA.Consumer.Api.Tests.Tests.ControllerTests
{
    public class DeviceManagementControllerTests
    {
        [Fact(DisplayName = "Get Image Success")]
        public async void GetImage_Success()
        {
            var serviceMock = new Mock<IDeviceManagementService>(MockBehavior.Strict);
            var imageResponse = TestDataHelper.GenerateRtspFeedResponse();
            serviceMock.Setup(m => m.GetRtspFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .ReturnsAsync(imageResponse);

            var controller = GetController(serviceMock);
            var result = await controller.GetRtspFeed("", "");

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(imageResponse, (result as OkObjectResult).Value as RtspFeedModel);
        }

        [Fact(DisplayName = "Get Image Invalid Token")]
        public async System.Threading.Tasks.Task GetImage_InvalidToken()
        {
            var exceptionMessage = "Invalid claim from token.";
            var serviceMock = new Mock<IDeviceManagementService>(MockBehavior.Strict);
            serviceMock.Setup(m => m.GetRtspFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .Throws(new Exception(exceptionMessage));

            var controller = GetController(serviceMock);
            var result = await controller.GetRtspFeed("", "");
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.BadRequest, (result as BadRequestObjectResult).StatusCode);
            Assert.Equal(exceptionMessage, (result as BadRequestObjectResult).Value);
        }

        [Fact(DisplayName = "Get Image Invalid User")]
        public async System.Threading.Tasks.Task GetImage_InvalidUser()
        {
            var exceptionMessage = "User not authorized to access device!";
            var serviceMock = new Mock<IDeviceManagementService>(MockBehavior.Strict);
            serviceMock.Setup(m => m.GetRtspFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .Throws(new Exception(exceptionMessage));

            var controller = GetController(serviceMock);
            var result = await controller.GetRtspFeed("", "");
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.BadRequest, (result as BadRequestObjectResult).StatusCode);
            Assert.Equal(exceptionMessage, (result as BadRequestObjectResult).Value);
        }

        [Fact(DisplayName = "Get Image No Access Token")]
        public async System.Threading.Tasks.Task GetImage_NoAccessToken()
        {
            var exceptionMessage = "Device Management access token not received!";
            var serviceMock = new Mock<IDeviceManagementService>(MockBehavior.Strict);
            serviceMock.Setup(m => m.GetRtspFeed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .Throws(new Exception(exceptionMessage));

            var controller = GetController(serviceMock);
            var result = await controller.GetRtspFeed("", "");
            Assert.Equal(typeof(BadRequestObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.BadRequest, (result as BadRequestObjectResult).StatusCode);
            Assert.Equal(exceptionMessage, (result as BadRequestObjectResult).Value);
        }

        private DeviceManagementController GetController(Mock<IDeviceManagementService> serviceMock) 
        {
            var controller = new DeviceManagementController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.HttpContext.Request.Headers.Authorization = TestDataHelper.GenerateJwtToken();
            return controller;
        }
    }
}
