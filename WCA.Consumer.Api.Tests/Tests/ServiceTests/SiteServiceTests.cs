using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;
using Xunit;

namespace WCA.Customer.Api.Tests
{
    public class SiteServiceTests
    {
        [Fact]
        public async void GetSitesForCustomer_Success()
        {
            var mySites = TestDataHelper.CreateSites(1);
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Site>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mySites);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await siteService.GetSitesForCustomer(mySites.First().CustomerId);

            Assert.Equal(typeof(List<SiteModel>), result.GetType());
            Assert.Equal(result[0].SiteId, mySites.First().SiteId);
            Assert.Equal(result[0].CustomerId, mySites.First().CustomerId);
        }

        [Fact]
        public async void GetSitesForCustomer_Fail()
        {
            var customerId = "customer-id";

            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<IList<Site>>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.GetSitesForCustomer(customerId));
            Assert.Contains("NotFound, Error: sth wrong", exception.Message);
        }

        [Fact]
        public async void GetSite_Success()
        {
            var mySite = TestDataHelper.CreateSite();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<Site>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mySite);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await siteService.GetSite(mySite.SiteId, mySite.CustomerId);

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

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.GetAsync<Site>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.GetSite(siteId, customerId));
            Assert.Contains("Error code: NotFound, Error: sth wrong", exception.Message);
        }

        [Fact]
        public async void CreateSite_Success()
        {
            var newSiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            Site mappedSite = new Site() { CustomerId = newSiteModel.CustomerId, SiteId = newSiteModel.SiteId };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PostAsync<Site>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mappedSite);

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);
            var result = await siteService.CreateSite(newSiteModel);

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, newSiteModel?.SiteId);
            Assert.Equal(result.CustomerId, newSiteModel?.CustomerId);
        }

        [Fact]
        public async void CreateSite_Fail()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PostAsync<Site>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.CreateSite(mySiteModel));
            Assert.Contains("Error code: NotFound, Error: sth wrong", exception.Message);
        }

        [Fact]
        public async void UpdateSite_Success()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            Site mappedSite = new Site() { CustomerId = mySiteModel.CustomerId, SiteId = mySiteModel.SiteId };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PutAsync<Site>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mappedSite);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await siteService.UpdateSite(mySiteModel.SiteId, mySiteModel);

            Assert.Equal(typeof(SiteModel), result.GetType());
            Assert.Equal(result.SiteId, mySiteModel?.SiteId);
            Assert.Equal(result.CustomerId, mySiteModel?.CustomerId);
        }

        [Fact]
        public async void UpdateSite_Fail()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.PutAsync<Site>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.UpdateSite(mySiteModel.SiteId, mySiteModel));
            Assert.Contains("Error code: NotFound, Error: sth wrong", exception.Message);
        }

        [Fact]
        public void DeleteSite_Success()
        {
            var mySiteModel = TestDataHelper.CreateSiteModel();
            var appSettings = TestDataHelper.CreateAppSettings();

            Site mappedSite = new Site() { CustomerId = mySiteModel.CustomerId, SiteId = mySiteModel.SiteId };

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.DeleteAsync<Site>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(mappedSite);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

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

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.DeleteAsync<Site>(It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException($"Error code: { HttpStatusCode.NotFound}, Error: sth wrong"));

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<SiteService>>();

            SiteService siteService = new SiteService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                siteService.DeleteSite(siteId));
            Assert.Contains("Error code: NotFound, Error: sth wrong", exception.Message);
        }
    }
}
