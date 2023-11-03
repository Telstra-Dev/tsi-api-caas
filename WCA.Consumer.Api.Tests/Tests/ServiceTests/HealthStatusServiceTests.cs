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
using Telstra.Core.Data.Models;
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

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, null, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, true);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = deviceModel.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = deviceModel.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(deviceModel.EdgeDevice)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(deviceModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", deviceModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Camera_Offline()
        {
            var deviceModel = TestDataHelper.CreateCameraModel();
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = deviceModel.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = deviceModel.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", deviceModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Online_NoLeafDevices()
        {
            var deviceModel = TestDataHelper.CreateGatewayModel();

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, true);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", deviceModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Offline_NoLeafDevices()
        {
            var deviceModel = TestDataHelper.CreateGatewayModel();

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(deviceModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", deviceModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Online_LeafDevices_Online()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, camera.DeviceId, true);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gatewayModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Offline_LeafDevices_Online()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, camera.DeviceId, true);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gatewayModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Online_LeafDevices_Offline()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, camera.DeviceId, false);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gatewayModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Gateway_Offline_LeafDevices_Offline()
        {
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, camera.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gatewayModel.DeviceId).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_NoGateways_NoLeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No gateways",
                Action = "Configure in site menu"
            };

            var httpClientMock = new Mock<IRestClient>();

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ArrayList());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No gateways",
                Action = "Config in site menu"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_NoGateways_LeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var cameraModel = TestDataHelper.CreateCameraModel();
            var devices = new ArrayList();
            devices.Add(cameraModel);

            var httpClientMock = new Mock<IRestClient>();

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No gateways",
                Action = "Config in site menu"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Online_NoLeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var devices = new ArrayList();
            devices.Add(gatewayModel);

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Review devices",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Offline_NoLeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var devices = new ArrayList();
            devices.Add(gatewayModel);

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Online_LeafDevices_Online()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var cameraModel = TestDataHelper.CreateCameraModel();

            var devices = new ArrayList();
            devices.Add(gatewayModel);
            devices.Add(cameraModel);

            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            camera.DeviceId = cameraModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, true);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(cameraModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Site online",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Offline_LeafDevices_Online()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var cameraModel = TestDataHelper.CreateCameraModel();

            var devices = new ArrayList();
            devices.Add(gatewayModel);
            devices.Add(cameraModel);

            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            camera.DeviceId = cameraModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, true);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(cameraModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Online_LeafDevices_Offline()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var cameraModel = TestDataHelper.CreateCameraModel();

            var devices = new ArrayList();
            devices.Add(gatewayModel);
            devices.Add(cameraModel);

            var camera = TestDataHelper.CreateDevice(DeviceType.camera);
            camera.EdgeDeviceId = gatewayModel.DeviceId;
            camera.DeviceId = cameraModel.DeviceId;
            var leafDevices = new List<Device>()
            {
                camera
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, false);
            var leafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera.DeviceId,
                RequiresConfiguration = false,
            };
            var leafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(cameraModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(leafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Offline_LeafDevices_Offline()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var cameraModel = TestDataHelper.CreateCameraModel();
            var devices = new ArrayList();
            devices.Add(gatewayModel);
            devices.Add(cameraModel);

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gatewayModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gatewayHealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(cameraModel.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(cameraHealthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            var expected = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            };
            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_1GatewayOffline_1GatewayCamerasOffline_1GatewayConfigureCameras_1GatewayDataOffline_1GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            // var camera1Model = cameraModels[0];
            // var camera2Model = cameraModels[1];
            // var camera3Model = cameraModels[2];
            // var camera4Model = cameraModels[3];
            // var camera5Model = cameraModels[4];
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            devices.Add(gateway2Model);
            devices.Add(gateway3Model);
            devices.Add(gateway4Model);
            devices.Add(gateway5Model);
            devices.Add(gateway6Model);
            // devices.Add(camera1Model);
            // devices.Add(camera2Model);
            // devices.Add(camera3Model);
            // devices.Add(camera4Model);
            // devices.Add(camera5Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // var result = healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result;

            // var expected = new HealthStatusModel
            // {
            //     Code = HealthStatusCode.RED,
            //     Reason = "Devices are offline",
            //     Action = "Expand site to review"
            // };
            // Assert.Equal(typeof(HealthStatusModel), result.GetType());
            // Assert.Equal(expected, result);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            // var gateway1Model = gatewayModels[0]; // Online
            // var gateway2Model = gatewayModels[1]; // Offline
            // var gateway3Model = gatewayModels[2]; // Cameras offline
            // var gateway4Model = gatewayModels[3]; // Configure Cameras
            // var gateway5Model = gatewayModels[4]; // Data offline
            // var gateway6Model = gatewayModels[5]; // No Cameras
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_0GatewayOffline_1GatewayCamerasOffline_1GatewayConfigureCameras_1GatewayDataOffline_1GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            // devices.Add(gateway2Model);
            devices.Add(gateway3Model);
            devices.Add(gateway4Model);
            devices.Add(gateway5Model);
            devices.Add(gateway6Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Devices are offline",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_0GatewayOffline_0GatewayCamerasOffline_1GatewayConfigureCameras_1GatewayDataOffline_1GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            // devices.Add(gateway2Model);
            // devices.Add(gateway3Model);
            devices.Add(gateway4Model);
            devices.Add(gateway5Model);
            devices.Add(gateway6Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Review devices",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_0GatewayOffline_0GatewayCamerasOffline_0GatewayConfigureCameras_1GatewayDataOffline_1GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            // devices.Add(gateway2Model);
            // devices.Add(gateway3Model);
            // devices.Add(gateway4Model);
            devices.Add(gateway5Model);
            devices.Add(gateway6Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Review devices",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_0GatewayOffline_0GatewayCamerasOffline_0GatewayConfigureCameras_0GatewayDataOffline_1GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            // devices.Add(gateway2Model);
            // devices.Add(gateway3Model);
            // devices.Add(gateway4Model);
            // devices.Add(gateway5Model);
            devices.Add(gateway6Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Review devices",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_1GatewayOnline_0GatewayOffline_0GatewayCamerasOffline_0GatewayConfigureCameras_0GatewayDataOffline_0GatewayNoCameras()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModels = TestDataHelper.CreateGatewayModels(6);
            var gateway1Model = gatewayModels[0]; // Online
            var gateway2Model = gatewayModels[1]; // Offline
            var gateway3Model = gatewayModels[2]; // Cameras offline
            var gateway4Model = gatewayModels[3]; // Configure Cameras
            var gateway5Model = gatewayModels[4]; // Data offline
            var gateway6Model = gatewayModels[5]; // No Cameras
            var cameraModels = TestDataHelper.CreateCameraModels(5);
            var devices = new ArrayList();
            devices.Add(gateway1Model);
            // devices.Add(gateway2Model);
            // devices.Add(gateway3Model);
            // devices.Add(gateway4Model);
            // devices.Add(gateway5Model);
            // devices.Add(gateway6Model);
            var cameras = TestDataHelper.CreateDevices(5, DeviceType.camera);
            var camera1 = cameras[0];
            var camera2 = cameras[1];
            var camera3 = cameras[2];
            var camera4 = cameras[3];
            var camera5 = cameras[4];
            camera1.EdgeDeviceId = gateway1Model.DeviceId;
            camera2.EdgeDeviceId = gateway2Model.DeviceId;
            camera3.EdgeDeviceId = gateway3Model.DeviceId;
            camera4.EdgeDeviceId = gateway4Model.DeviceId;
            camera5.EdgeDeviceId = gateway5Model.DeviceId;

            var gateway1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, gateway1Model.DeviceId, true);
            var gateway2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, gateway2Model.DeviceId, false);
            var gateway3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, gateway3Model.DeviceId, true);
            var gateway4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, gateway4Model.DeviceId, true);
            var gateway5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, gateway5Model.DeviceId, true);
            var gateway6HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway6Model.EdgeDevice, gateway6Model.DeviceId, true);
            var camera1HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway1Model.EdgeDevice, camera1.DeviceId, true);
            var camera2HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway2Model.EdgeDevice, camera2.DeviceId, true);
            var camera3HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway3Model.EdgeDevice, camera3.DeviceId, false);
            var camera4HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway4Model.EdgeDevice, camera4.DeviceId, false);
            var camera5HealthDataStatus = TestDataHelper.CreateHealthDataStatus(gateway5Model.EdgeDevice, camera5.DeviceId, true);
            var camera1LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera1.DeviceId,
                RequiresConfiguration = false,
            };
            var camera1LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera1.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera2LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera2.DeviceId,
                RequiresConfiguration = false,
            };
            var camera2LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera2.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
            };
            var camera3LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera3.DeviceId,
                RequiresConfiguration = false,
            };
            var camera3LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera3.DeviceId,
                Timestamp = null,
            };
            var camera4LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera4.DeviceId,
                RequiresConfiguration = true,
            };
            var camera4LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera4.DeviceId,
                Timestamp = null,
            };
            var camera5LeafDeviceConfigurationStatus = new LeafDeviceConfigurationStatus
            {
                LeafDeviceId = camera5.DeviceId,
                RequiresConfiguration = false,
            };
            var camera5LeafDeviceLatestTelemetryStatus = new LeafDeviceLatestTelemetryStatus
            {
                LeafDeviceId = camera5.DeviceId,
                Timestamp = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-5)).ToUnixTimeMilliseconds(),
            };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway1Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway2Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway3Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway4Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway5Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(gateway6Model.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(gateway6HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera1.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera2.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera3.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera4.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith(camera5.DeviceId)),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5HealthDataStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera1.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera1LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera2.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera2LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera3.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera3LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera4.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera4LeafDeviceLatestTelemetryStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceConfigurationStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/configurationStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceConfigurationStatus);
            httpClientMock.Setup(x => x.SendAsync<LeafDeviceLatestTelemetryStatus>(
                It.Is<HttpRequestMessage>(r => r.RequestUri.ToString().EndsWith($"{camera5.DeviceId}/latestTelemetryStatus")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(camera5LeafDeviceLatestTelemetryStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway1Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway1Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway2Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway2Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway3Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway3Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway4Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway4Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway5Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway5Model);
            deviceServiceMock.Setup(m => m.GetDevice("fake.user.email@example.com", gateway6Model.DeviceId, It.IsAny<string>())).ReturnsAsync(gateway6Model);
            deviceServiceMock.Setup(m => m.GetDevices("fake.user.email@example.com", It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway1Model.DeviceId)).ReturnsAsync(new List<Device>() { camera1 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway2Model.DeviceId)).ReturnsAsync(new List<Device>() { camera2 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway3Model.DeviceId)).ReturnsAsync(new List<Device>() { camera3 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway4Model.DeviceId)).ReturnsAsync(new List<Device>() { camera4 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway5Model.DeviceId)).ReturnsAsync(new List<Device>() { camera5 });
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway("fake.user.email@example.com", gateway6Model.DeviceId)).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            // Camera / leaf device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = null,
                Action = null
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera1).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera2).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera3).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure camera",
                Action = "Config in camera menu"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera4).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Contact support"
            }, healthStatusService.GetDeviceHealthStatus("fake.user.email@example.com", camera5).Result);

            // Gateway / edge device checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway1Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway2Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera(s) offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway3Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Configure cameras",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway4Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "Data offline",
                Action = "Expand gateway to review"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway5Model.DeviceId).Result);
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Config in gateway menu"
            }, healthStatusService.GetHealthStatusFromDeviceId("fake.user.email@example.com", gateway6Model.DeviceId).Result);

            // Site checks.
            Assert.Equal(new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Site online",
                Action = "Expand site to review"
            }, healthStatusService.GetSiteHealthStatus("fake.user.email@example.com", site).Result);
        }
    }
}
