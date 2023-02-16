using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
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
    public class OrganisationServiceTests
    {
        [Fact]
        public void GetOrganisationOverview_Success()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim("email", "user@example.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            var jwtString = jwtHandler.WriteToken(jwt);

            var myOrganisations = TestDataHelper.CreateOrganisations(1);
            var mySites = TestDataHelper.CreateSites(1);
            var myDevices = TestDataHelper.CreateDevices(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisations);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations/overview")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var responseJsonSites = JsonConvert.SerializeObject(mySites);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites")
                    .Respond("application/json", responseJsonSites.ToString());
            var responseJsonDevices = JsonConvert.SerializeObject(myDevices);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond("application/json", responseJsonDevices.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.GetOrganisationOverview(jwtString).Result;

            Assert.Equal(typeof(List<OrgSearchTreeNode>), result.GetType());
            Assert.Equal(result[0].Id, myOrganisations.First().CustomerId);
            Assert.Equal(result[1].Id, mySites.First().SiteId);
            Assert.Equal(result[2].Id, myDevices.First().DeviceId);
        }

        [Fact]
        public void GetOrganisationOverview_Success_WCC()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim("email", "wcc-user@telstrasmartspacesdemo.onmicrosoft.com")
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
            myOrganisations.Add(myOrganisationNonWcc);
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
            mySites.Add(mySiteNonWcc);
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
            myDevices.Add(myDeviceNonWcc);
            myDevices.Add(myDeviceWcc);

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisations);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations/overview")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var responseJsonSites = JsonConvert.SerializeObject(mySites);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites")
                    .Respond("application/json", responseJsonSites.ToString());
            var responseJsonDevices = JsonConvert.SerializeObject(myDevices);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond("application/json", responseJsonDevices.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.GetOrganisationOverview(jwtString).Result;

            Assert.Equal(typeof(List<OrgSearchTreeNode>), result.GetType());
            Assert.Equal(result[0].Id, customerIdWcc);
            Assert.Equal(result[1].Id, siteIdWcc);
            Assert.Equal(result[2].Id, deviceIdWcc);
        }

        [Fact]
        public async void GetOrganisationOverview_Fail()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim("email", "user@example.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            var jwtString = jwtHandler.WriteToken(jwt);

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations/overview")
                    .Respond(HttpStatusCode.NotFound);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites")
                    .Respond(HttpStatusCode.NotFound);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/devices")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                organisationService.GetOrganisationOverview(jwtString));
            Assert.Equal("Error getting org overview. NotFound Response code from downstream: NotFound", exception.Message);
        }

        [Fact]
        public void GetOrganisation_Success()
        {
            var myOrganisations = TestDataHelper.CreateOrganisations(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisations);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations?customerId={myOrganisations.First().CustomerId}")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.GetOrganisation(myOrganisations.First().CustomerId, false).Result;

            Assert.Equal(typeof(OrganisationModel), result.GetType());
            Assert.Equal(result.Id, myOrganisations.First().CustomerId);
        }

        [Fact]
        public async void GetOrganisation_Fail()
        {
            var customerId = "customer-id";
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations?customerId={customerId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                organisationService.GetOrganisation(customerId, false));
            Assert.Equal("Error getting an organisation. NotFound Response code from downstream: NotFound", exception.Message);
        }

        [Fact]
        public void CreateOrganisation_Success()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisationModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.CreateOrganisation(myOrganisationModel).Result;

            Assert.Equal(typeof(OrganisationModel), result.GetType());
            Assert.Equal(result.CustomerId, myOrganisationModel?.CustomerId);
        }

        [Fact]
        public async void CreateOrganisation_Fail()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                organisationService.CreateOrganisation(myOrganisationModel));
            Assert.Equal("Error creating an organisation. NotFound Response code from downstream: NotFound", exception.Message);
        }

        [Fact]
        public void UpdateOrganisation()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisationModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.UpdateOrganisation(myOrganisationModel.CustomerId, myOrganisationModel);

            Assert.Equal(typeof(OrganisationModel), result.GetType());
            Assert.Equal(result.CustomerId, myOrganisationModel?.CustomerId);
        }

        [Fact]
        public void DeleteOrganisation()
        {
            var myOrganisationModel = TestDataHelper.CreateOrganisationModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJsonOrgs = JsonConvert.SerializeObject(myOrganisationModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/organisations")
                    .Respond("application/json", responseJsonOrgs.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            OrganisationService organisationService = new OrganisationService(null, httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = organisationService.DeleteOrganisation(myOrganisationModel.Id);

            Assert.Equal(typeof(OrganisationModel), result.GetType());
        }
    }
}
