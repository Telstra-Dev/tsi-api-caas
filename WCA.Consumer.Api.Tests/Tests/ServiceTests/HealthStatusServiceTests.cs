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

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, true);

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

            var result = healthStatusService.GetHealthStatusFromDeviceId(deviceModel.DeviceId).Result;

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

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, false);

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

            var result = healthStatusService.GetHealthStatusFromDeviceId(deviceModel.DeviceId).Result;

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
                Reason = "No cameras",
                Action = "Configure in gateway menu"
            };

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, true);

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

            var result = healthStatusService.GetHealthStatusFromDeviceId(deviceModel.DeviceId).Result;

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

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(deviceModel.EdgeDevice, deviceModel.DeviceId, false);

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

            var result = healthStatusService.GetHealthStatusFromDeviceId(deviceModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review"
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

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
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

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Expand gateway to review"
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

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Contact support"
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

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetHealthStatusFromDeviceId(gatewayModel.DeviceId).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ArrayList());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_NoGateways_LeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var cameraModel = TestDataHelper.CreateCameraModel();
            var devices = new ArrayList();
            devices.Add(cameraModel);

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
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Online_NoLeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var devices = new ArrayList();
            devices.Add(gatewayModel);

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.AMBER,
                Reason = "No cameras",
                Action = "Expand site to review"
            };

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
        }

        [Fact]
        public void GetDeviceHealthStatus_Site_Gateways_Offline_NoLeafDevices()
        {
            var site = TestDataHelper.CreateSite();
            var gatewayModel = TestDataHelper.CreateGatewayModel();
            var devices = new ArrayList();
            devices.Add(gatewayModel);

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Expand site to review"
            };

            var healthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<HealthDataStatus>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(healthDataStatus);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(new List<Device>());

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Site online",
                Action = "Expand site to review"
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, true);

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
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Expand site to review"
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, false);
            var cameraHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, cameraModel.DeviceId, true);

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
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Camera offline",
                Action = "Expand site to review"
            };

            var gatewayHealthDataStatus = TestDataHelper.CreateHealthDataStatus(gatewayModel.EdgeDevice, gatewayModel.DeviceId, true);
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
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(leafDevices);

            var healthStatusService = new HealthStatusService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object,
                                                              deviceServiceMock.Object, null, cacheMock.Object);

            var result = healthStatusService.GetSiteHealthStatus(site).Result;

            Assert.Equal(typeof(HealthStatusModel), result.GetType());
            Assert.Equal(result.Reason, healthStatusModel.Reason);
            Assert.Equal(result.Action, healthStatusModel.Action);
            Assert.Equal(result.Code, healthStatusModel.Code);
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

            var healthStatusModel = new HealthStatusModel
            {
                Code = HealthStatusCode.RED,
                Reason = "Gateway offline",
                Action = "Expand site to review"
            };

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
            deviceServiceMock.Setup(m => m.GetDevice(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(gatewayModel);
            deviceServiceMock.Setup(m => m.GetDevices(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);
            deviceServiceMock.Setup(m => m.GetLeafDevicesForGateway(It.IsAny<string>())).ReturnsAsync(new List<Device>());

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
