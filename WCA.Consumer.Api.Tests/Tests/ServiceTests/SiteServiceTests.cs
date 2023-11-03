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
                siteService.GetSitesForCustomer("fake.user.email@example.com", customerId));
            Assert.Contains("NotFound, Error: sth wrong", exception.Message);
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
                siteService.GetSite("fake.user.email@example.com", siteId, customerId));
            Assert.Contains("Error code: NotFound, Error: sth wrong", exception.Message);
        }
    }
}
