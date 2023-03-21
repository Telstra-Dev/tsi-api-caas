using System.Collections;
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
    public class DeviceControllerTests
    {
        [Fact(DisplayName = "Get devices")]
        public async void GetDevices_Success()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateDeviceModel();
            var myDevices = new ArrayList() { myDevice };
            serviceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(myDevices);

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.GetDevices(myDevice.CustomerId, myDevice.SiteId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            ArrayList expectedDevices = ((result as OkObjectResult).Value as ArrayList);
            var expectedDevice = (DeviceModel)expectedDevices[0];
            Assert.Equal(expectedDevice.DeviceId, myDevice.DeviceId);
        }

        [Fact(DisplayName = "Get devices not found")]
        public async void GetDevices_NotFound()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var customerId = "customer-id";
            var siteId = "site-id";
            serviceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((ArrayList)null);

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.GetDevices(customerId, siteId);

            Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundObjectResult).StatusCode);
        }

        [Fact(DisplayName = "Get one device")]
        public async void GetDevice_Success()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateDeviceModel();
            serviceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(myDevice);

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.GetDevice(myDevice.DeviceId, myDevice.CustomerId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            DeviceModel expectedDevice = ((result as OkObjectResult).Value as DeviceModel);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }

        [Fact(DisplayName = "Get device not found")]
        public async void GetDevice_NotFound()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var customerId = "customer-id";
            var deviceId = "device-id";
            serviceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<DeviceModel>(null));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.GetDevice(deviceId, customerId);

            Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundObjectResult).StatusCode);
        }

        [Fact(DisplayName = "Delete device")]
        public async void DeleteDevice()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateDeviceModel();
            serviceMock.Setup(m => m.DeleteDevice(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(myDevice));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.DeleteDevice(myDevice.CustomerId, myDevice.DeviceId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            DeviceModel expectedDevice = ((result as OkObjectResult).Value as DeviceModel);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }

        [Fact(DisplayName = "Create Camera Device")]
        public async void CreateCameraDevice()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateCameraModel();
            serviceMock.Setup(m => m.CreateCameraDevice(It.IsAny<Camera>())).Returns(Task.FromResult(myDevice));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.CreateCameraDevice(myDevice);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Camera expectedDevice = ((result as OkObjectResult).Value as Camera);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }

        [Fact(DisplayName = "Update Camera Device")]
        public async void UpdateCameraDevice()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateCameraModel();
            serviceMock.Setup(m => m.UpdateCameraDevice(It.IsAny<string>(), It.IsAny<Camera>())).Returns(Task.FromResult(myDevice));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.UpdateCameraDevice(myDevice.DeviceId, myDevice);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Camera expectedDevice = ((result as OkObjectResult).Value as Camera);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }

        [Fact(DisplayName = "Create Edge Device")]
        public async void CreateEdgeDevice()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateGatewayModel();
            serviceMock.Setup(m => m.CreateEdgeDevice(It.IsAny<Gateway>())).Returns(Task.FromResult(myDevice));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.CreateEdgeDevice(myDevice);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Gateway expectedDevice = ((result as OkObjectResult).Value as Gateway);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }

        [Fact(DisplayName = "Update Camera Device")]
        public async void UpdateEdgeDevice()
        {
            var serviceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            var myDevice = TestDataHelper.CreateGatewayModel();
            serviceMock.Setup(m => m.UpdateEdgeDevice(It.IsAny<string>(), It.IsAny<Gateway>())).Returns(Task.FromResult(myDevice));

            var controller = new DeviceController(serviceMock.Object);

            var result = await controller.UpdateEdgeDevice(myDevice.DeviceId, myDevice);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Gateway expectedDevice = ((result as OkObjectResult).Value as Gateway);
            Assert.Equal(expectedDevice.DeviceId, myDevice?.DeviceId);
        }
    }
}
