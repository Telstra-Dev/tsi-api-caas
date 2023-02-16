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
    public class SiteServiceTests
    {
        [Fact]
        public void GetSitesForCustomer_Success()
        {
            var mySites = TestDataHelper.CreateSiteModels(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(mySites);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites?customerId={mySites.First().CustomerId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = siteService.GetSitesForCustomer(mySites.First().CustomerId).Result;

            Assert.Equal(typeof(List<SiteModel>), result.GetType());
            Assert.Equal(result[0].SiteId, mySites.First().SiteId);
            Assert.Equal(result[0].CustomerId, mySites.First().CustomerId);
        }

        [Fact]
        public async void GetSitesForCustomer_Fail()
        {
            var customerId = "customer-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites?customerId={customerId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.GetSitesForCustomer(customerId));
            Assert.Equal("Error getting sites for customerId. Response code from downstream: NotFound", exception.Message);
        }

        [Fact]
        public void GetSite_Success()
        {
            var mySite = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(mySite);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{mySite.SiteId}?customerId={mySite.CustomerId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = siteService.GetSite(mySite.SiteId, mySite.CustomerId).Result;

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, mySite?.SiteId);
            Assert.Equal(result.CustomerId, mySite?.CustomerId);
        }

        [Fact]
        public async void GetSite_Fail()
        {
            var customerId = "customer-id";
            var siteId = "site-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{siteId}?customerId={customerId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.GetSite(siteId, customerId));
            Assert.Equal("Error getting site. Response code from downstream: NotFound", exception.Message);
        }

        [Fact]
        public void CreateSite_Success()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(mySiteModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = siteService.CreateSite(mySiteModel).Result;

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, mySiteModel?.SiteId);
            Assert.Equal(result.CustomerId, mySiteModel?.CustomerId);
        }

        [Fact]
        public async void CreateSite_Fail()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.CreateSite(mySiteModel));
            Assert.Equal("Error saving a site. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void UpdateSite_Success()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(mySiteModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{mySiteModel.SiteId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = siteService.UpdateSite(mySiteModel.SiteId, mySiteModel).Result;

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, mySiteModel?.SiteId);
            Assert.Equal(result.CustomerId, mySiteModel?.CustomerId);
        }

        [Fact]
        public async void UpdateSite_Fail()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{mySiteModel.SiteId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.UpdateSite(mySiteModel.SiteId, mySiteModel));
            Assert.Equal("Error saving a site. NotFound Response code from downstream: ", exception.Message);
        }

        [Fact]
        public void DeleteSite_Success()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(mySiteModel);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{mySiteModel.SiteId}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = siteService.DeleteSite(mySiteModel.SiteId).Result;

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, mySiteModel?.SiteId);
            Assert.Equal(result.CustomerId, mySiteModel?.CustomerId);
        }

        [Fact]
        public async void DeleteSite_Fail()
        {
            var siteId = "site-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/sites/{siteId}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SiteService siteService = new SiteService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.DeleteSite(siteId));
            Assert.Equal("Error deleting a site. NotFound Response code from downstream: ", exception.Message);
        }
    }
}
