using AutoMapper;
using Moq;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.AutomapperProfiles;
using WCA.Consumer.Api.Models;
using Xunit;

namespace WCA.Customer.Api.Tests
{ 
    public class OrgOverviewMappingProfileTests
    {
        [Fact]
        public void OrganisationMappingProfile_ConfigIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrganisationMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void OrgOverviewMappingProfile_ConfigIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrgOverviewMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void OrgOverviewMappingProfile_Organisation_OrgSearchTreeNode_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrgOverviewMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var organisation = TestDataHelper.CreateOrganisation();

            var result = mapper.Map<Organisation, OrgSearchTreeNode>(organisation);

            Assert.Equal(result.Type, "organisation");
            Assert.Equal(result.Href, $"/organisations?customerId={organisation.CustomerId}");
        }

        [Fact]
        public void OrgOverviewMappingProfile_Site_OrgSearchTreeNode_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrgOverviewMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var site = TestDataHelper.CreateSite();

            var result = mapper.Map<Site, OrgSearchTreeNode>(site);

            Assert.Equal(result.Type, "site");
            Assert.Equal(result.Href, $"/sites?customerId={site.CustomerId}");
        }

        [Fact]
        public void OrgOverviewMappingProfile_Gateway_Device_OrgSearchTreeNode_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrgOverviewMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var device = TestDataHelper.CreateDevice(DeviceType.gateway);

            var result = mapper.Map<Device, OrgSearchTreeNode>(device);

            Assert.Equal(result.ParentId, device.SiteId);
            Assert.Equal(result.Type, device.Type.ToString());
            Assert.Equal(result.Href, $"/devices/{device.DeviceId}");
        }

        [Fact]
        public void OrgOverviewMappingProfile_Camera_Device_OrgSearchTreeNode_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<OrgOverviewMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var device = TestDataHelper.CreateDevice(DeviceType.camera);

            var result = mapper.Map<Device, OrgSearchTreeNode>(device);

            Assert.Equal(result.ParentId, device.EdgeDeviceId);
            Assert.Equal(result.Type, device.Type.ToString());
            Assert.Equal(result.Href, $"/devices/{device.DeviceId}");
        }
    }
}