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

            var origTags = siteModel.Metadata.Tags;
            var resultTags = result.Tags.ToList();

            for (int i = 0; i < origTags.Count; i++)
            {
                var origKey = origTags.ElementAt(i).Key;
                var origValues = origTags.ElementAt(i).Value;

                var resultValues = resultTags.Where(t => t.TagName == origKey).Select(t => t.TagValue).ToList();
                Assert.Equal(resultValues.Count, origValues.Count());

                for (int j = 0; j < origValues.Count(); j++)
                {
                    Assert.Equal(resultValues[j].ToString(), origValues[j]);
                }
            }
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

            var origTags = site.Tags.ToList();
            var resultTags = result.Metadata.Tags;

            for (int i = 0; i < origTags.Count; i++)
            {
                var origKey = origTags[i].TagName;
                var origValues = origTags.Where(t => t.TagName == origKey).Select(t => t.TagValue).ToList();

                Assert.Equal(resultTags[origKey].Count(), origValues.Count());

                for (int j = 0; j < origValues.Count(); j++)
                {
                    Assert.Equal(resultTags[origKey][j], origValues[j]);
                }
            }
        }
    }
}