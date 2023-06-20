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
    public class OrganisationServiceTests
    {
        [Fact]
        public async Task GetOrganisationOverview_Success()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = "b2clogin";
            var claims = new List<Claim>
            {
                new Claim("email", "someone@team.telstra.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            var jwtString = jwtHandler.WriteToken(jwt);

            var myOrganisations = TestDataHelper.CreateOrganisations(1);
            var mySites = TestDataHelper.CreateSites(1);
            var myDevices = TestDataHelper.CreateDevices(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Organisation>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myOrganisations);
            httpClientMock.Setup(x => x.GetAsync<IList<Site>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mySites);
            httpClientMock.Setup(x => x.GetAsync<IList<Device>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myDevices);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var result = await organisationService.GetOrganisationOverview(jwtString);

            Assert.Equal(typeof(List<OrgSearchTreeNode>), result.GetType());
            Assert.Equal(result[0].Id, myOrganisations.First().CustomerId);
            Assert.Equal(result[1].Id, mySites.First().SiteId);
            Assert.Equal(result[2].Id, myDevices.First().DeviceId);
        }

        [Fact]
        public async Task GetOrganisationOverview_Success_WCC()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = "b2clogin";
            var claims = new List<Claim>
            {
                new Claim("email", "wcc-admin@telstrasmartspacesdemo.onmicrosoft.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            var jwtString = jwtHandler.WriteToken(jwt);

            var customerIdNonWcc = "abc-id";
            var customerIdWcc = "wcc-id";
            var siteIdNonWcc = "site-id-abc";
            var siteIdWcc = "site-id-wcc";
            var deviceIdNonWcc = "device-id-abc";
            var deviceIdWcc = "device-id-wcc";
            var myOrganisations = new List<Organisation>();
            var myOrganisationNonWcc = new Organisation
            {
                Id = customerIdNonWcc,
                CustomerId = customerIdNonWcc,
            };
            var myOrganisationWcc = new Organisation
            {
                Id = customerIdWcc,
                CustomerId = customerIdWcc,
            };
            myOrganisations.Add(myOrganisationWcc);
            var mySites = new List<Site>();
            var mySiteNonWcc = new Site
            {
                SiteId = siteIdNonWcc,
                CustomerId = customerIdNonWcc,
            };
            var mySiteWcc = new Site
            {
                SiteId = siteIdWcc,
                CustomerId = customerIdWcc,
            };
            mySites.Add(mySiteWcc);
            var myDevices = new List<Device>();
            var myDeviceNonWcc = new Device
            {
                DeviceId = deviceIdNonWcc,
                SiteId = siteIdNonWcc,
                CustomerId = customerIdNonWcc,
            };
            var myDeviceWcc = new Device
            {
                DeviceId = deviceIdWcc,
                SiteId = siteIdWcc,
                CustomerId = customerIdWcc,
            };
            myDevices.Add(myDeviceWcc);

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Organisation>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myOrganisations);
            httpClientMock.Setup(x => x.GetAsync<IList<Site>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mySites);
            httpClientMock.Setup(x => x.GetAsync<IList<Device>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myDevices);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var result = await organisationService.GetOrganisationOverview(jwtString);

            Assert.Equal(typeof(List<OrgSearchTreeNode>), result.GetType());
            Assert.Equal(result[0].Id, customerIdWcc);
            Assert.Equal(result[1].Id, siteIdWcc);
            Assert.Equal(result[2].Id, deviceIdWcc);
        }

        [Fact]
        public async void GetOrganisationOverview_EmptyResult()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = "b2clogin";
            var claims = new List<Claim>
            {
                new Claim("email", "user@unknown.domain.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            var jwtString = jwtHandler.WriteToken(jwt);

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Organisation>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Organisation>());
            httpClientMock.Setup(x => x.GetAsync<IList<Site>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Site>());
            httpClientMock.Setup(x => x.GetAsync<IList<Device>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Device>());


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            Assert.Empty(await organisationService.GetOrganisationOverview(jwtString));

        }

        [Fact]
        public async void GetOrganisation_Success()
        {
            var myOrganisations = TestDataHelper.CreateOrganisations(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Organisation>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myOrganisations);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var result = await organisationService.GetOrganisation(myOrganisations.First().CustomerId, false);

            Assert.Equal(typeof(OrganisationModel), result.GetType());
            Assert.Equal(result.Id, myOrganisations.First().CustomerId);
        }

        [Fact]
        public async void GetOrganisation_Fail()
        {
            var customerId = "customer-id";
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Organisation>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                organisationService.GetOrganisation(customerId, false));
            Assert.Contains("Error code: NotFound", exception.Message);
        }

        [Fact]
        public async void CreateOrganisation_Success()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PostAsync<OrganisationModel>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(myOrganisationModel);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var result = await organisationService.CreateOrganisation(myOrganisationModel);

            Assert.Equal(typeof(OrganisationModel), result.GetType());
            Assert.Equal(result.CustomerId, myOrganisationModel?.CustomerId);
        }

        [Fact]
        public async void CreateOrganisation_Fail()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PostAsync<OrganisationModel>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();
            var healthStatusServiceMock = new Mock<IHealthStatusService>();

            var organisationService = new OrganisationService(null, httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                organisationService.CreateOrganisation(myOrganisationModel));
            Assert.Contains("Error code: NotFound", exception.Message);
        }

        //[Fact]
        //public void UpdateOrganisation()
        //{
        //    var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
        //    var appSettings = TestDataHelper.CreateAppSettings();

        //    var mockHttp = new MockHttpMessageHandler();
        //    var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisationModel);
        //    mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
        //            .Respond("application/json", responseJsonOrgs.ToString());
        //    var httpClientMock = mockHttp.ToHttpClient();

        //    var mapperMock = TestDataHelper.CreateMockMapper();
        //    var loggerMock = new Mock<ILogger<OrganisationService>>();
        //    var healthStatusServiceMock = new Mock<IHealthStatusService>();

        //    var organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

        //    var result = organisationService.UpdateOrganisation(myOrganisationModel.CustomerId, myOrganisationModel);

        //    Assert.Equal(typeof(OrganisationModel), result.GetType());
        //    Assert.Equal(result.CustomerId, myOrganisationModel?.CustomerId);
        //}

        //[Fact]
        //public void DeleteOrganisation()
        //{
        //    var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
        //    var appSettings = TestDataHelper.CreateAppSettings();

        //    var mockHttp = new MockHttpMessageHandler();
        //    var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisationModel);
        //    mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
        //            .Respond("application/json", responseJsonOrgs.ToString());
        //    var httpClientMock = mockHttp.ToHttpClient();

        //    var mapperMock = TestDataHelper.CreateMockMapper();
        //    var loggerMock = new Mock<ILogger<OrganisationService>>();
        //    var healthStatusServiceMock = new Mock<IHealthStatusService>();

        //    var organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object, healthStatusServiceMock.Object);

        //    var result = organisationService.DeleteOrganisation(myOrganisationModel.Id);

        //    Assert.Equal(typeof(OrganisationModel), result.GetType());
        //}
    }
}
