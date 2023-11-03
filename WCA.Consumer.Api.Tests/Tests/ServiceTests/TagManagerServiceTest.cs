using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Customer.Api.Tests;
using Xunit;

namespace WCA.Consumer.Api.Tests.Tests.ServiceTests
{
    public class TagManagerServiceTest
    {
        [Fact(DisplayName = "Get Tags")]
        public async void GetTags_Success()
        {
            var tags = TestDataHelper.CreateTags(5);

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            mockResponse.Content = new StringContent(JsonConvert.SerializeObject(tags));

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                                        .ReturnsAsync(mockResponse);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<TagManagerService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var tagManagerServiceMock = new TagManagerService(httpClientMock.Object, 
                                                                appSettings, 
                                                                mapperMock.Object, 
                                                                loggerMock.Object, 
                                                                cacheMock.Object);

            var result = await tagManagerServiceMock.GetTagsAsync("fake.user.email@example.com");

            Assert.NotNull(result);
            Assert.Equal(tags.Count, result.Count);
            Assert.Equal(tags[0].Id, result[0].Id);
        }

        [Fact(DisplayName = "Create Tags")]
        public async void CreateTags_Success()
        {
            var tags = TestDataHelper.CreateTags(2);
            var createTagPayload = TestDataHelper.GenerateCreateTagsPayload(2);

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            mockResponse.Content = new StringContent(JsonConvert.SerializeObject(tags.Count));

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                                        .ReturnsAsync(mockResponse);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<TagManagerService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var tagManagerServiceMock = new TagManagerService(httpClientMock.Object,
                                                                appSettings,
                                                                mapperMock.Object,
                                                                loggerMock.Object,
                                                                cacheMock.Object);

            var result = await tagManagerServiceMock.CreateTagsAsync("fake.user.email@example.com", createTagPayload);

            Assert.Equal(createTagPayload.Count, result);
        }

        [Fact(DisplayName = "Rename Tag")]
        public async void RenameTag_Success()
        {
            var tags = TestDataHelper.CreateTags(1);

            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK);
            mockResponse.Content = new StringContent(JsonConvert.SerializeObject(tags[0]));

            var httpClientMock = new Mock<IRestClient>();
            httpClientMock.Setup(x => x.SendWithResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                                        .ReturnsAsync(mockResponse);

            var appSettings = TestDataHelper.CreateAppSettings();
            var mapperMock = TestDataHelper.CreateMockMapper();
            var loggerMock = new Mock<ILogger<TagManagerService>>();
            var cacheMock = new Mock<IMemoryCache>();

            var tagManagerServiceMock = new TagManagerService(httpClientMock.Object,
                                                                appSettings,
                                                                mapperMock.Object,
                                                                loggerMock.Object,
                                                                cacheMock.Object);

            var result = await tagManagerServiceMock.RenameTagAsync("fake.user.email@example.com", tags[0]);

            Assert.NotNull(result);
            Assert.Equal(tags[0].Name, result.Name);
        }
    }
}
