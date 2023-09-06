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
    }
}