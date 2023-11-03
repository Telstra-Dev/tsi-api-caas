using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using WCA.Consumer.Api.Models;
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
            var serialNumbers = TestDataHelper.CreateSerialNumbers(1);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<string>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serialNumbers);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            var service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, null, null);

            var result = service.GetSerialNumberByValue("fake.user.email@example.com", serialNumber).Result;

            Assert.Equal(typeof(SerialNumberModel), result.GetType());
            Assert.Equal(result.Value, serialNumber);
        }

        [Fact]
        public async void GetSerialNumberByValue_NonExistent()
        {
            var serialNumber = TestDataHelper.CreateSerialNumber();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<string>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IList<string>>(null));

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            var service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, null, null);

            var result = await service.GetSerialNumberByValue("fake.user.email@example.com", serialNumber);

            Assert.Null(result);
        }

        [Fact]
        public async void GetSerialNumbersByFilter_SingleMatch()
        {
            var devices = TestDataHelper.CreateDevices(1);
            var serialNumbers = TestDataHelper.CreateSerialNumbers(1);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<string>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serialNumbers);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetGatewayDevices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);

            var service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, deviceServiceMock.Object, null);

            var result = await service.GetSerialNumbersByFilter("fake.user.email@example.com", serialNumber);

            Assert.Equal(typeof(List<SerialNumberModel>), result.GetType());
            var expectedSerialNumbers = result.ToList();
            Assert.Single(expectedSerialNumbers);
            Assert.Equal(expectedSerialNumbers[0].Value, serialNumber);
        }

        [Fact]
        public async void GetSerialNumbersByFilter_MultipleMatches()
        {
            var count = 5;
            var devices = TestDataHelper.CreateDevices(count);
            var serialNumbers = TestDataHelper.CreateSerialNumbers(count);
            var serialNumber = serialNumbers.First();
            var appSettings = TestDataHelper.CreateAppSettings();
            var searchFilter = serialNumber.Substring(0, 5);

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<string>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serialNumbers);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetGatewayDevices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);

            var service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, deviceServiceMock.Object, null);

            var result = await service.GetSerialNumbersByFilter("fake.user.email@example.com", searchFilter);

            Assert.Equal(typeof(List<SerialNumberModel>), result.GetType());
            var expectedSerialNumbers = result.ToList();
            Assert.Equal(expectedSerialNumbers.Count, count);
            for (int i = 0; i < serialNumbers.Count; i++)
            {
                Assert.Equal(expectedSerialNumbers[i].Value, serialNumbers[i]);
            }
        }

        [Fact]
        public async void GetSerialNumbersByFilter_NonExistent()
        {
            var devices = TestDataHelper.CreateDevices(0);
            var appSettings = TestDataHelper.CreateAppSettings();
            var emptyList = new List<string>();

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendAsync<IList<string>>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<OrganisationService>>();

            var deviceServiceMock = new Mock<IDeviceService>(MockBehavior.Strict);
            deviceServiceMock.Setup(m => m.GetGatewayDevices(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(devices);

            var service = new SerialNumberService(httpClientMock.Object, appSettings, mapperMock.Object, loggerMock.Object, deviceServiceMock.Object, null);

            var result = await service.GetSerialNumbersByFilter("fake.user.email@example.com", "non-existent");

            Assert.Null(result);
        }
    }
}
