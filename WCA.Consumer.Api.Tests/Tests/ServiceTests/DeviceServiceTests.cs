using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;
using Xunit;

namespace WCA.Customer.Api.Tests
{
    public class DeviceServiceTests
    {
        [Fact]
        public async void GetDevices_Success()
        {
            var myGateway = TestDataHelper.CreateDevice(DeviceType.gateway);
            var myCamera = TestDataHelper.CreateDevice(DeviceType.camera);
            var devices = new List<Device>();
            devices.Add(myGateway);
            devices.Add(myCamera);
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<Device>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(devices);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await deviceService.GetDevices(customerId, siteId);

            Assert.Equal(typeof(ArrayList), result.GetType());
            var resultGateway = (Gateway)result[0];
            var resultCamera = (Camera)result[1];
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);
            Assert.Equal(resultCamera.DeviceId, myCamera?.DeviceId);
            Assert.Equal(resultCamera.Type, DeviceType.camera);
        }

        [Fact]
        public async void GetDevices_Fail()
        {
            var errMsg = "something goes wrong";
            var customerId = "customer-id";
            var siteId = "site-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<Device>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception(errMsg));


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.GetDevices(customerId, siteId));
            Assert.Equal(errMsg, exception.Message);
        }

        [Fact]
        public async void GetDevice_Gateway_Success()
        {
            var myGateway = TestDataHelper.CreateDevice(DeviceType.gateway);
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<Device>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(myGateway);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var resultGateway = await deviceService.GetDevice(myGateway.DeviceId, customerId);

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);
        }

        [Fact]
        public async void GetDevice_NotFound()
        {
            var customerId = "customer-id";
            var deviceIdGateway = "device-id-gateway";

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<Device>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync((Device)null);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);
            var result = await deviceService.GetDevice(deviceIdGateway, customerId);

            Assert.Null(result);
        }

        [Fact]
        public async void DeleteGateway_Success()
        {
            var appSettings = TestDataHelper.CreateAppSettings();
            var myGateway = TestDataHelper.CreateDevice(DeviceType.gateway);
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            HttpContent content = new StringContent(JsonConvert.SerializeObject(myGateway));
            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var resultGateway = await deviceService.DeleteDevice(customerId, myGateway.DeviceId);

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);
        }

        [Fact]
        public async void DeleteDevice_Fail()
        {
            var appSettings = TestDataHelper.CreateAppSettings();
            var customerId = "customer-id";
            var deviceIdGateway = "device-id-gateway";

            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("") };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.DeleteDevice(customerId, deviceIdGateway));
            Assert.Equal("Error deleting a device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public async void CreateCameraDevice_Success()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(myCamera));
            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await deviceService.CreateCameraDevice(myCamera);

            Assert.Equal(typeof(Camera), result.GetType());
            Assert.Equal(result.DeviceId, myCamera?.DeviceId);
            Assert.Equal(result.Type, DeviceType.camera);
        }

        [Fact]
        public async void UpdateCameraDevice_Success()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(myCamera));
            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await deviceService.UpdateCameraDevice(myCamera.DeviceId, myCamera);

            Assert.Equal(typeof(Camera), result.GetType());
            Assert.Equal(result.DeviceId, myCamera?.DeviceId);
            Assert.Equal(result.Type, DeviceType.camera);
        }


        [Fact]
        public async void CreateCameraDevice_Fail()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("") };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.CreateCameraDevice(myCamera));
            Assert.Equal("Error creating an edge device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public async void CreateEdgeDevice_Success()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(myGateway));
            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await deviceService.CreateEdgeDevice(myGateway);

            Assert.Equal(typeof(Gateway), result.GetType());
            Assert.Equal(result.DeviceId, myGateway?.DeviceId);
            Assert.Equal(result.Type, DeviceType.gateway);
        }

        [Fact]
        public async void UpdateEdgeDevice()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            HttpContent content = new StringContent(JsonConvert.SerializeObject(myGateway));
            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await deviceService.UpdateEdgeDevice(myGateway.DeviceId, myGateway);

            Assert.Equal(typeof(Gateway), result.GetType());
            Assert.Equal(result.DeviceId, myGateway?.DeviceId);
            Assert.Equal(result.Type, DeviceType.gateway);
        }

        [Fact]
        public async void CreateEdgeDevice_Fail()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpResponseMsg = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound, Content = new StringContent("") };
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(httpResponseMsg);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.CreateEdgeDevice(myGateway));
            Assert.Equal("Error saving an edge device. NotFound Response code from downstream: ", exception.Message);
        }
    }
}
