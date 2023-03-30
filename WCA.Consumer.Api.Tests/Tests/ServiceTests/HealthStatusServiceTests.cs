using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    public class HealthStatusServiceTests
    {
        [Fact]
        public void GetDeviceHealthStatus_Camera_Online()
        {
            var deviceModel = TestDataHelper.CreateCameraModel();

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = deviceModel.EdgeDevice,
                EdgeLeafdeviceid = deviceModel.DeviceId,
                EdgeStarttime = DateTime.UtcNow,
                EdgeEndtime = DateTime.UtcNow,
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(deviceModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Null(result.Reason);
            Assert.Null(result.Action);
            Assert.Equal(result.Code, HealthStatusCode.GREEN);
        }

        [Fact]
        public void GetDeviceHealthStatus_Camera_Offline()
        {
            var deviceModel = TestDataHelper.CreateCameraModel();
            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            };

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = deviceModel.EdgeDevice,
                EdgeLeafdeviceid = deviceModel.DeviceId,
                EdgeStarttime = new DateTime(2000, 3, 29, 10, 0, 0),
                EdgeEndtime = new DateTime(2000, 3, 29, 10, 0, 0),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(deviceModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Online_NoLeafDevices()
        {
            var deviceModel = TestDataHelper.CreateGatewayModel();
            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras attached",
                Action = "Configure on gateway"
            };

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = deviceModel.EdgeDevice,
                EdgeLeafdeviceid = deviceModel.DeviceId,
                EdgeStarttime = DateTime.UtcNow,
                EdgeEndtime = DateTime.UtcNow,
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);


            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(deviceModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Offline_NoLeafDevices()
        {
            var deviceModel = TestDataHelper.CreateGatewayModel();
            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            };

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = deviceModel.EdgeDevice,
                EdgeLeafdeviceid = deviceModel.DeviceId,
                EdgeStarttime = new DateTime(2000, 3, 29, 10, 0, 0),
                EdgeEndtime = new DateTime(2000, 3, 29, 10, 0, 0),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(deviceModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Online_LeafDevices()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = gatewayModel.EdgeDevice,
                EdgeLeafdeviceid = camera.DeviceId,
                EdgeStarttime = DateTime.UtcNow,
                EdgeEndtime = DateTime.UtcNow,
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Null(result.Reason);
            Assert.Null(result.Action);
            Assert.Equal(result.Code, HealthStatusCode.GREEN);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Offline_LeafDevices()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            };

            var healthDataStatus = new HealthDataStatus
            {
                SvId = 1,
                EdgeEdgedeviceid = gatewayModel.EdgeDevice,
                EdgeLeafdeviceid = gatewayModel.DeviceId,
                EdgeStarttime = new DateTime(2000, 3, 29, 10, 0, 0),
                EdgeEndtime = new DateTime(2000, 3, 29, 10, 0, 0),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetDeviceHealthStatus(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_NoDevices()
        {
            var site = TestDataHelper.CreateSite();
            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No devices",
                Action = "Configure in site menu"
            };

            var httpClientMock = new Mock<IRestClient>();

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ArrayList());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }
    }
}
