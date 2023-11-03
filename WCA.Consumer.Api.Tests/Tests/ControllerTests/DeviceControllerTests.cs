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
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace WCA.Customer.Api.Tests
{
    public class DeviceControllerTests
    {
        [Fact(DisplayName = "Get Edge Devices")]
        public async void GetEdgeDevices_Success()
        {
            var edgeDevices = TestDataHelper.CreateEdgeDevices(3);
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.GetEdgeDevices(It.IsAny<string>())).ReturnsAsync(edgeDevices);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.GetEdgeDevices("fake.user.email@example.com");
            var expectedDevices = (result as OkObjectResult).Value as List<EdgeDeviceModel>;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(edgeDevices.Count, expectedDevices.Count);
            Assert.Equal(edgeDevices[0].EdgeEdgedeviceid, expectedDevices[0].EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Get Leaf Devices")]
        public async void GetLeafDevices_Success()
        {
            var leafDevices = TestDataHelper.CreateLeafDevices(3);
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.GetLeafDevices(It.IsAny<string>())).ReturnsAsync(leafDevices);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.GetLeafDevices("fake.user.email@example.com");
            var expectedDevices = (result as OkObjectResult).Value as List<LeafDeviceModel>;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(leafDevices.Count, expectedDevices.Count);
            Assert.Equal(leafDevices[0].EdgeLeafdeviceid, leafDevices[0].EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Get Edge Device by Id")]
        public async void GetEdgeDeviceById_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.GetEdgeDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(edgeDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.GetEdgeDevice("fake.user.email@example.com", edgeDevice.EdgeEdgedeviceid);
            var expectedDevice = (result as OkObjectResult).Value as EdgeDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, expectedDevice.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Get Leaf Device by Id")]
        public async void GetLeafDeviceById_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.GetLeafDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(leafDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.GetLeafDevice("fake.user.email@example.com", leafDevice.EdgeLeafdeviceid);
            var expectedDevice = (result as OkObjectResult).Value as LeafDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, expectedDevice.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Create Edge Device")]
        public async void CreateEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.CreateEdgeDevice(It.IsAny<string>(), It.IsAny<EdgeDeviceModel>())).ReturnsAsync(edgeDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.CreateEdgeDevice("fake.user.email@example.com", edgeDevice);
            var expectedDevice = (result as OkObjectResult).Value as EdgeDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, expectedDevice.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Update Edge Device")]
        public async void UpdateEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.UpdateTsiEdgeDevice(It.IsAny<string>(), It.IsAny<EdgeDeviceModel>())).ReturnsAsync(edgeDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.UpdateEdgeDevice("fake.user.email@example.com", edgeDevice);
            var expectedDevice = (result as OkObjectResult).Value as EdgeDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, expectedDevice.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Delete Edge Device")]
        public async void DeleteEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.DeleteEdgeDevice(It.IsAny<string>(), It.IsAny<EdgeDeviceModel>())).ReturnsAsync(edgeDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.DeleteEdgeDevice("fake.user.email@example.com", edgeDevice);
            var expectedDevice = (result as OkObjectResult).Value as EdgeDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, expectedDevice.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Create Leaf Device")]
        public async void CreateLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.CreateLeafDevice(It.IsAny<string>(), It.IsAny<LeafDeviceModel>())).ReturnsAsync(leafDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.CreateLeafDevice("fake.user.email@example.com", leafDevice);
            var expectedDevice = (result as OkObjectResult).Value as LeafDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, expectedDevice.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Update Leaf Device")]
        public async void UpdateLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.UpdateLeafDevice(It.IsAny<string>(), It.IsAny<LeafDeviceModel>())).ReturnsAsync(leafDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.UpdateLeafDevice("fake.user.email@example.com", leafDevice);
            var expectedDevice = (result as OkObjectResult).Value as LeafDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, expectedDevice.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Delete Leaf Device")]
        public async void DeleteLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var serviceMock = new Mock<IDeviceService>();
            serviceMock.Setup(m => m.DeleteLeafDevice(It.IsAny<string>(), It.IsAny<LeafDeviceModel>())).ReturnsAsync(leafDevice);
            var controller = new DeviceController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            // controller.HttpContext.Request.Headers["X-CUsername"] = "fake.user.email@example.com"; // TODO: check if this injection is actually required.
            var result = await controller.DeleteLeafDevice("fake.user.email@example.com", leafDevice);
            var expectedDevice = (result as OkObjectResult).Value as LeafDeviceModel;

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, expectedDevice.EdgeLeafdeviceid);
        }
    }
}
