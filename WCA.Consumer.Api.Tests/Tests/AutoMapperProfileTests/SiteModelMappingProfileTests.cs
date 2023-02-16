using System.Linq;
using AutoMapper;
using Moq;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.AutomapperProfiles;
using WCA.Consumer.Api.Models;
using Xunit;

namespace WCA.Customer.Api.Tests
{ 
    public class SiteModelMappingProfileTests
    {
        [Fact]
        public void SiteModelMappingProfile_ConfigIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SiteModelMappingProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void SiteModelMappingProfile_SiteModel_Site_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SiteModelMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var siteModel = TestDataHelper.CreateSiteModel();

            var result = mapper.Map<SiteModel, Site>(siteModel);

            Assert.Equal(result.StoreCode, siteModel.Metadata.StoreCode);
            Assert.Equal(result.State, siteModel.Metadata.State);
            Assert.Equal(result.Type, siteModel.Metadata.Type);
            Assert.Equal(result.StoreFormat, siteModel.Metadata.StoreFormat);
            Assert.Equal(result.GeoClassification, siteModel.Metadata.GeoClassification);
            Assert.Equal(result.Region, siteModel.Metadata.Region);
            Assert.Equal(result.OrganisationId, siteModel.CustomerId);
            Assert.Equal(result.Location.Latitude, siteModel.Location.GeoLocation.Latitude);
            Assert.Equal(result.Location.Longitude, siteModel.Location.GeoLocation.Longitude);

            Assert.Equal(result.Tags.ToList()[0].TagName, siteModel.Metadata.Tags.First().Key);
            Assert.Equal(result.Tags.ToList()[0].TagValue, siteModel.Metadata.Tags.First().Value.First());
        }

        [Fact]
        public void SiteModelMappingProfile_Site_SiteModel_IsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SiteModelMappingProfile>());
            config.AssertConfigurationIsValid();
            var mapper = config.CreateMapper();

            var site = TestDataHelper.CreateSite();

            var result = mapper.Map<Site, SiteModel>(site);

            Assert.Equal(result.Metadata.StoreCode, site.StoreCode);
            Assert.Equal(result.Metadata.State, site.State);
            Assert.Equal(result.Metadata.Type, site.Type);
            Assert.Equal(result.Metadata.StoreFormat, site.StoreFormat);
            Assert.Equal(result.Metadata.GeoClassification, site.GeoClassification);
            Assert.Equal(result.Metadata.Region, site.Region);
            Assert.Equal(result.CustomerId, site.OrganisationId);
            Assert.Equal(result.Location.GeoLocation.Latitude, site.Location.Latitude);
            Assert.Equal(result.Location.GeoLocation.Longitude, site.Location.Longitude);

            Assert.Equal(result.Metadata.Tags.First().Key, site.Tags.ToList()[0].TagName);
            Assert.Equal(result.Metadata.Tags.First().Value.First(), site.Tags.ToList()[0].TagValue);
        }
    }
}