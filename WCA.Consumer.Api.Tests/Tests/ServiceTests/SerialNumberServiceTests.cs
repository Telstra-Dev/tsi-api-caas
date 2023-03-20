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
    public class SerialNumberServiceTests
    {
        [Fact]
        public void GetSerialNumberByValue_Success()
        {
            var serialNumber = TestDataHelper.CreateSerialNumber();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<SerialNumber>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(serialNumber);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = service.GetSerialNumberByValue(serialNumber.Value).Result;

            Assert.Equal(typeof(SerialNumberModel), result.GetType());
            Assert.Equal(result.SerialNumberId, serialNumber.SerialNumberId);
            Assert.Equal(result.Value, serialNumber.Value);
            Assert.Equal(result.DeviceId, serialNumber.DeviceId);
        }

        [Fact]
        public async void GetSerialNumberByValue_NonExistent()
        {
            var serialNumber = TestDataHelper.CreateSerialNumber();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<SerialNumber>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync((SerialNumber)null);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await service.GetSerialNumberByValue(serialNumber.Value);

            Assert.Null(result);
        }

        [Fact]
        public async void GetSerialNumbersByFilter_SingleMatch()
        {
            var serialNumbers = TestDataHelper.CreateSerialNumbers(1);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<SerialNumber>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(serialNumbers);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await service.GetSerialNumbersByFilter(serialNumber.Value);

            Assert.Equal(typeof(List<SerialNumberModel>), result.GetType());
            var expectedSerialNumbers = result.ToList();
            Assert.Single(expectedSerialNumbers);
            Assert.Equal(expectedSerialNumbers[0].SerialNumberId, serialNumber.SerialNumberId);
            Assert.Equal(expectedSerialNumbers[0].Value, serialNumber.Value);
            Assert.Equal(expectedSerialNumbers[0].DeviceId, serialNumber.DeviceId);
        }

        [Fact]
        public async void GetSerialNumbersByFilter_MultipleMatches()
        {
            var count = 5;
            var serialNumbers = TestDataHelper.CreateSerialNumbers(count);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();
            var searchFilter = serialNumber.Value.Substring(0, 10);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<SerialNumber>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).ReturnsAsync(serialNumbers);


            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var result = await service.GetSerialNumbersByFilter(searchFilter);

            Assert.Equal(typeof(List<SerialNumberModel>), result.GetType());
            var expectedSerialNumbers = result.ToList();
            Assert.Equal(expectedSerialNumbers.Count, count);
            for (int i = 0; i < serialNumbers.Count; i++)
            {
                Assert.Equal(expectedSerialNumbers[i].SerialNumberId, serialNumbers[i].SerialNumberId);
                Assert.Equal(expectedSerialNumbers[i].Value, serialNumbers[i].Value);
                Assert.Equal(expectedSerialNumbers[i].DeviceId, serialNumbers[i].DeviceId);
            }
        }

        [Fact]
        public async void GetSerialNumbersByFilter_NonExistent()
        {
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<SerialNumber>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.GetSerialNumbersByFilter("non-existent"));
            Assert.Contains("GetSerialNumbers failed", exception.Message);
        }
    }
}
