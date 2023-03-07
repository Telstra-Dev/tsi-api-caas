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
    public class SerialNumberServiceTests
    {
        [Fact]
        public void GetSerialNumberByValue_Success()
        {
            var serialNumber = TestDataHelper.CreateSerialNumber();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(serialNumber);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/serialNumbers?value={serialNumber.Value}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

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

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(serialNumber);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/serialNumbers?value={serialNumber.Value}")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = service.GetSerialNumberByValue(serialNumber.Value).Result;

            Assert.Null(result);
        }

        [Fact]
        public void GetSerialNumbersByFilter_SingleMatch()
        {
            var serialNumbers = TestDataHelper.CreateSerialNumbers(1);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(serialNumbers);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/serialNumbers?filter={serialNumber.Value}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = service.GetSerialNumbersByFilter(serialNumber.Value).Result;

            Assert.Equal(typeof(List<SerialNumberModel>), result.GetType());
            var expectedSerialNumbers = result.ToList();
            Assert.Equal(expectedSerialNumbers.Count, 1);
            Assert.Equal(expectedSerialNumbers[0].SerialNumberId, serialNumber.SerialNumberId);
            Assert.Equal(expectedSerialNumbers[0].Value, serialNumber.Value);
            Assert.Equal(expectedSerialNumbers[0].DeviceId, serialNumber.DeviceId);
        }

        [Fact]
        public void GetSerialNumbersByFilter_MultipleMatches()
        {
            var count = 5;
            var serialNumbers = TestDataHelper.CreateSerialNumbers(count);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();
            var searchFilter = serialNumber.Value.Substring(0, 10);

            var mockHttp = new MockHttpMessageHandler();
            var responseJson = JsonConvert.SerializeObject(serialNumbers);
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/serialNumbers?filter={searchFilter}")
                    .Respond("application/json", responseJson.ToString());
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var result = service.GetSerialNumbersByFilter(searchFilter).Result;

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

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{appSettings.StorageAppHttp.BaseUri}/serialNumbers*")
                    .Respond(HttpStatusCode.NotFound);
            var httpClientMock = mockHttp.ToHttpClient();

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            SerialNumberService service = new SerialNumberService(httpClientMock, appSettings, mapperMock.Object, loggerMock.Object);

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                service.GetSerialNumbersByFilter("non-existent"));
            Assert.Equal("Error getting serial number. NotFound Response code from downstream: ", exception.Message);
        }
    }
}
