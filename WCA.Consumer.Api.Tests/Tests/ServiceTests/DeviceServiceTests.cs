using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            var result = await deviceService.GetDevices("fake.user.email@example.com", customerId, siteId.ToString());

            Assert.Equal(typeof(ArrayList), result.GetType());
            var resultGateway = (Gateway)result[0];
            var resultCamera = (Camera)result[1];

            Assert.Equal(resultGateway.Type, DeviceType.gateway);

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
                deviceService.GetDevices("fake.user.email@example.com", customerId, siteId));
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

            var resultGateway = await deviceService.GetDevice("fake.user.email@example.com", myGateway.DeviceId.ToString(), customerId);

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            //Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
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
            var result = await deviceService.GetDevice("fake.user.email@example.com", deviceIdGateway, customerId);

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

            var resultGateway = await deviceService.DeleteDevice("fake.user.email@example.com", customerId, myGateway.DeviceId.ToString());

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            //Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
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
                deviceService.DeleteDevice("fake.user.email@example.com", customerId, deviceIdGateway));
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

            var result = await deviceService.CreateCameraDevice("fake.user.email@example.com", myCamera);

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

            var result = await deviceService.UpdateCameraDevice("fake.user.email@example.com", myCamera.DeviceId, myCamera);

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
                deviceService.CreateCameraDevice("fake.user.email@example.com", myCamera));
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

            var result = await deviceService.CreateEdgeDevice("fake.user.email@example.com", myGateway);

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

            var result = await deviceService.UpdateEdgeDevice("fake.user.email@example.com", myGateway.DeviceId, myGateway);

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
                deviceService.CreateEdgeDevice("fake.user.email@example.com", myGateway));
            Assert.Equal("Error saving an edge device. NotFound Response code from downstream: ", exception.Message);
        }

        // TSI Test Cases

        [Fact(DisplayName = "Get Edge Devices")]
        public async void GetEdgeDevices_Success()
        {
            var edgeDevices = TestDataHelper.CreateEdgeDevices(3);
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => 
                                    x.SendAsync<List<EdgeDeviceModel>>(It.IsAny<HttpRequestMessage>(),
                                                                        It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(edgeDevices);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.GetEdgeDevices("fake.user.email@example.com");

            Assert.Equal(typeof(List<EdgeDeviceModel>), result.GetType());
            Assert.Equal(edgeDevices.Count, result.Count);
            Assert.Equal(edgeDevices.ElementAt(1).EdgeEdgedeviceid, result.ElementAt(1).EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Get Leaf Devices")]
        public async void GetLeafDevices_Success()
        {
            var leafDevices = TestDataHelper.CreateLeafDevices(3);
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<List<LeafDeviceModel>>(It.IsAny<HttpRequestMessage>(),
                                                                        It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(leafDevices);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.GetLeafDevices("fake.user.email@example.com");

            Assert.Equal(typeof(List<LeafDeviceModel>), result.GetType());
            Assert.Equal(leafDevices.Count, result.Count);
            Assert.Equal(leafDevices.ElementAt(1).EdgeLeafdeviceid, result.ElementAt(1).EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Get Edge Device by Id")]
        public async void GetEdgeDeviceById_Success()
        {
            var edgeDevices = TestDataHelper.CreateEdgeDevices(3);
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<EdgeDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                        It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(edgeDevices.ElementAt(1));

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.GetEdgeDevice("fake.user.email@example.com", edgeDevices.ElementAt(1).EdgeEdgedeviceid);

            Assert.NotNull(result);
            Assert.Equal(typeof(EdgeDeviceModel), result.GetType());
            Assert.Equal(edgeDevices.ElementAt(1).Id, result.Id);
            Assert.Equal(edgeDevices.ElementAt(1).Name, result.Name);
            Assert.Equal(edgeDevices.ElementAt(1).SiteId, result.SiteId);
            Assert.Equal(edgeDevices.ElementAt(1).EdgeEdgedeviceid, result.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Get Leaf Device by Id")]
        public async void GetLeafDeviceById_Success()
        {
            var leafDevices = TestDataHelper.CreateLeafDevices(3);
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<LeafDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                        It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(leafDevices.ElementAt(1));

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.GetLeafDevice("fake.user.email@example.com", leafDevices.ElementAt(1).EdgeLeafdeviceid);

            Assert.NotNull(result);
            Assert.Equal(typeof(LeafDeviceModel), result.GetType());
            Assert.Equal(leafDevices.ElementAt(1).Id, result.Id);
            Assert.Equal(leafDevices.ElementAt(1).Name, result.Name);
            Assert.Equal(leafDevices.ElementAt(1).EdgeLeafdeviceid, result.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Update Edge Device")]
        public async void UpdateEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<EdgeDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(edgeDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.UpdateTsiEdgeDevice("fake.user.email@example.com", edgeDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(EdgeDeviceModel), result.GetType());
            Assert.Equal(edgeDevice.Id, result.Id);
            Assert.Equal(edgeDevice.Name, result.Name);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, result.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Update Leaf Device")]
        public async void UpdateLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<LeafDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(leafDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.UpdateLeafDevice("fake.user.email@example.com", leafDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(LeafDeviceModel), result.GetType());
            Assert.Equal(leafDevice.Name, result.Name);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, leafDevice.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Create Edge Device")]
        public async void CreateTsiEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<EdgeDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(edgeDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.CreateEdgeDevice("fake.user.email@example.com", edgeDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(EdgeDeviceModel), result.GetType());
            Assert.Equal(edgeDevice.Id, result.Id);
            Assert.Equal(edgeDevice.Name, result.Name);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, result.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Delete Edge Device")]
        public async void DeleteEdgeDevice_Success()
        {
            var edgeDevice = TestDataHelper.CreateEdgeDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<EdgeDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(edgeDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.DeleteEdgeDevice("fake.user.email@example.com", edgeDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(EdgeDeviceModel), result.GetType());
            Assert.Equal(edgeDevice.Id, result.Id);
            Assert.Equal(edgeDevice.Name, result.Name);
            Assert.Equal(edgeDevice.EdgeEdgedeviceid, result.EdgeEdgedeviceid);
        }

        [Fact(DisplayName = "Create Leaf Device")]
        public async void CreateLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<LeafDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(leafDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.CreateLeafDevice("fake.user.email@example.com", leafDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(LeafDeviceModel), result.GetType());
            Assert.Equal(leafDevice.Name, result.Name);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, leafDevice.EdgeLeafdeviceid);
        }

        [Fact(DisplayName = "Delete Leaf Device")]
        public async void DeleteLeafDevice_Success()
        {
            var leafDevice = TestDataHelper.CreateLeafDevices(1)[0];
            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x =>
                                    x.SendAsync<LeafDeviceModel>(It.IsAny<HttpRequestMessage>(),
                                                                    It.IsAny<CancellationToken>()))
                                    .ReturnsAsync(leafDevice);

            var deviceService = GetDeviceService(httpClientMock);
            var result = await deviceService.DeleteLeafDevice("fake.user.email@example.com", leafDevice);

            Assert.NotNull(result);
            Assert.Equal(typeof(LeafDeviceModel), result.GetType());
            Assert.Equal(leafDevice.Name, result.Name);
            Assert.Equal(leafDevice.EdgeLeafdeviceid, leafDevice.EdgeLeafdeviceid);
        }

        private DeviceService GetDeviceService(Mock<IRestClient> httpClientMock)
        {
            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            return new DeviceService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);
        }
    }
}
