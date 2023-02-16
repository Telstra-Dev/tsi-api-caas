using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    public class DeviceServiceTests
    {
        [Fact]
        public void GetDevices_Success()
        {
            var myGateway = TestDataHelper.CreateDevice(DeviceType.gateway);
            var myCamera = TestDataHelper.CreateDevice(DeviceType.camera);
            var devices = new List<Device>();
            devices.Add(myGateway);
            devices.Add(myCamera);
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(devices);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = deviceService.GetDevices(customerId, siteId).Result;

            Assert.Equal(typeof(ArrayList), result.GetType());
            var resultGateway = (Gateway) result[0];
            var resultCamera = (Camera) result[1];
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);
            Assert.Equal(resultCamera.DeviceId, myCamera?.DeviceId);
            Assert.Equal(resultCamera.Type, DeviceType.camera);
        }

        [Fact]
        public async void GetDevices_Fail()
        {
            var customerId = "customer-id";
            var siteId = "site-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.GetDevices(customerId, siteId));
            Assert.Equal("Error getting devices. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void GetDevice_Success()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var myCamera = TestDataHelper.CreateCameraModel();
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            var appSettings = TestDataHelper.CreateAppSettings();
            
            var mockHttp = new MockHttpMessageHandler();
            var responseJsonGateway = JsonConvert.SerializeObject(myGateway);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices/{myGateway.DeviceId}?customerId={customerId}")
                    .Respond("application/json", responseJsonGateway.ToString());
            var responseJsonCamera = JsonConvert.SerializeObject(myCamera);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices/{myCamera.DeviceId}?customerId={customerId}")
                    .Respond("application/json", responseJsonCamera.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var resultGateway = deviceService.GetDevice(myGateway.DeviceId, customerId).Result;

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);

            var resultCamera = deviceService.GetDevice(myCamera.DeviceId, customerId).Result;

            Assert.Equal(typeof(Camera), resultCamera.GetType());
            Assert.Equal(resultCamera.DeviceId, myCamera?.DeviceId);
            Assert.Equal(resultCamera.Type, DeviceType.camera);
        }

        [Fact]
        public async void GetDevice_Fail()
        {
            var customerId = "customer-id";
            var deviceIdGateway = "device-id-gateway";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices/{deviceIdGateway}?customerId={customerId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.GetDevice(deviceIdGateway, customerId));
            Assert.Equal("Error getting device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void DeleteDevice_Success()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var myCamera = TestDataHelper.CreateCameraModel();
            var customerId = myGateway.CustomerId;
            var siteId = myGateway.SiteId;

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonGateway = JsonConvert.SerializeObject(myGateway);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&deviceId={myGateway.DeviceId}")
                    .Respond("application/json", responseJsonGateway.ToString());
            var responseJsonCamera = JsonConvert.SerializeObject(myCamera);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&deviceId={myCamera.DeviceId}")
                    .Respond("application/json", responseJsonCamera.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var resultGateway = deviceService.DeleteDevice(customerId, myGateway.DeviceId).Result;

            Assert.Equal(typeof(Gateway), resultGateway.GetType());
            Assert.Equal(resultGateway.DeviceId, myGateway?.DeviceId);
            Assert.Equal(resultGateway.Type, DeviceType.gateway);

            var resultCamera = deviceService.DeleteDevice(customerId, myCamera.DeviceId).Result;

            Assert.Equal(typeof(Camera), resultCamera.GetType());
            Assert.Equal(resultCamera.DeviceId, myCamera?.DeviceId);
            Assert.Equal(resultCamera.Type, DeviceType.camera);
        }

        [Fact]
        public async void DeleteDevice_Fail()
        {
            var customerId = "customer-id";
            var deviceIdGateway = "device-id-gateway";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&deviceId={deviceIdGateway}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.DeleteDevice(customerId, deviceIdGateway));
            Assert.Equal("Error deleting a device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void CreateCameraDevice_Success()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(myCamera);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = deviceService.CreateCameraDevice(myCamera).Result;

            Assert.Equal(typeof(Camera), result.GetType());
            Assert.Equal(result.DeviceId, myCamera?.DeviceId);
            Assert.Equal(result.Type, DeviceType.camera);
        }

        [Fact]
        public async void CreateCameraDevice_Fail()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.CreateCameraDevice(myCamera));
            Assert.Equal("Error creating an edge device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void UpdateCameraDevice()
        {
            var myCamera = TestDataHelper.CreateCameraModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(myCamera);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices/{myCamera.DeviceId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = deviceService.UpdateCameraDevice(myCamera.DeviceId, myCamera).Result;

            Assert.Equal(typeof(Camera), result.GetType());
            Assert.Equal(result.DeviceId, myCamera?.DeviceId);
            Assert.Equal(result.Type, DeviceType.camera);
        }

        [Fact]
        public void CreateEdgeDevice_Success()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(myGateway);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = deviceService.CreateEdgeDevice(myGateway).Result;

            Assert.Equal(typeof(Gateway), result.GetType());
            Assert.Equal(result.DeviceId, myGateway?.DeviceId);
            Assert.Equal(result.Type, DeviceType.gateway);
        }

        [Fact]
        public async void CreateEdgeDevice_Fail()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                deviceService.CreateEdgeDevice(myGateway));
            Assert.Equal("Error saving an edge device. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void UpdateEdgeDevice()
        {
            var myGateway = TestDataHelper.CreateGatewayModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(myGateway);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices/{myGateway.DeviceId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            DeviceService deviceService = new DeviceService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = deviceService.UpdateEdgeDevice(myGateway.DeviceId, myGateway).Result;

            Assert.Equal(typeof(Gateway), result.GetType());
            Assert.Equal(result.DeviceId, myGateway?.DeviceId);
            Assert.Equal(result.Type, DeviceType.gateway);
        }
    }
}
