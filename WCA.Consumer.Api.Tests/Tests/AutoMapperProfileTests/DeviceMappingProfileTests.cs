using AutoMapper;
using Moq;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.AutomapperProfiles;
using WCA.Consumer.Api.Models;
using Xunit;

namespace WCA.Customer.Api.Tests
{ 
    public class DeviceMappingProfileTests
    {
        [Fact]
        public void DeviceMappingProfile_ConfigIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DeviceMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void DeviceMappingProfile_Gateway_Device_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DeviceMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var gateway = TestDataHelper.CreateGatewayModel();

            var result = mapper.Map<Gateway, Device>(gateway);

            Assert.Equal(result.EdgeDeviceId, gateway.EdgeDevice);
            Assert.Equal(result.MetadataUrl, gateway.Metadata.Hub);
            Assert.Equal(result.IsEdgeCapable, gateway.EdgeCapable);
            Assert.Equal(result.IsActive, gateway.Active);
            Assert.Equal(result.MetadataHub, gateway.Metadata.Hub);
            Assert.Equal(result.MetadataAuthConnString, gateway.Metadata.Auth.IotHubConnectionString);
            Assert.Equal(result.MetadataAuthSymmetricKey, gateway.Metadata.Auth.SymmetricKey.PrimaryKey);
        }

        [Fact]
        public void DeviceMappingProfile_Device_Gateway_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DeviceMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var device = TestDataHelper.CreateDevice();

            var result = mapper.Map<Device, Gateway>(device);

            Assert.Equal(result.EdgeDevice, device.EdgeDeviceId);
            Assert.Equal(result.Metadata.Hub, device.MetadataHub);
            Assert.Equal(result.EdgeCapable, device.IsEdgeCapable);
            Assert.Equal(result.Active, device.IsActive);
            Assert.Equal(result.Metadata.Hub, device.MetadataHub);
            Assert.Equal(result.Metadata.Auth.IotHubConnectionString, device.MetadataAuthConnString);
            Assert.Equal(result.Metadata.Auth.SymmetricKey.PrimaryKey, device.MetadataAuthSymmetricKey);
        }

        [Fact]
        public void DeviceMappingProfile_Camera_Device_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DeviceMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var camera = TestDataHelper.CreateCameraModel();

            var result = mapper.Map<Camera, Device>(camera);

            Assert.Equal(result.EdgeDeviceId, camera.EdgeDevice);
            Assert.Equal(result.MetadataUrl, camera.Metadata.Url);
            Assert.Equal(result.IsEdgeCapable, camera.EdgeCapable);
            Assert.Equal(result.IsActive, camera.Active);
            Assert.Equal(result.MetadataUrl, camera.Metadata.Url);
            Assert.Equal(result.MetadataUsername, camera.Metadata.Username);
            Assert.Equal(result.MetadataPassword, camera.Metadata.Password);
            Assert.Equal(result.MetadataHub, camera.Metadata.Hub);
            Assert.Equal(result.MetadataAuthConnString, camera.Metadata.Auth.IotHubConnectionString);
            Assert.Equal(result.MetadataAuthSymmetricKey, camera.Metadata.Auth.SymmetricKey.PrimaryKey);
        }

        [Fact]
        public void DeviceMappingProfile_Device_Camera_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DeviceMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var device = TestDataHelper.CreateDevice();

            var result = mapper.Map<Device, Camera>(device);

            Assert.Equal(result.EdgeDevice, device.EdgeDeviceId);
            Assert.Equal(result.Metadata.Url, device.MetadataUrl);
            Assert.Equal(result.EdgeCapable, device.IsEdgeCapable);
            Assert.Equal(result.Active, device.IsActive);
            Assert.Equal(result.Metadata.Url, device.MetadataUrl);
            Assert.Equal(result.Metadata.Username, device.MetadataUsername);
            Assert.Equal(result.Metadata.Password, device.MetadataPassword);
            Assert.Equal(result.Metadata.Hub, device.MetadataHub);
            Assert.Equal(result.Metadata.Auth.IotHubConnectionString, device.MetadataAuthConnString);
            Assert.Equal(result.Metadata.Auth.SymmetricKey.PrimaryKey, device.MetadataAuthSymmetricKey);
        }
    }
}