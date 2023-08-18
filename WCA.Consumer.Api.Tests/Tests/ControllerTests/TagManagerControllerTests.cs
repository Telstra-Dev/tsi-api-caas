using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Controllers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Customer.Api.Tests;
using Xunit;

namespace WCA.Consumer.Api.Tests.Tests.ControllerTests
{
    public class TagManagerControllerTests
    {
        [Fact(DisplayName = "Get Tags")]
        public async void GetTags_Success()
        {
            var serviceMock = new Mock<ITagManagerService>(MockBehavior.Strict);
            var tags = TestDataHelper.CreateTags(5);
            serviceMock.Setup(m => m.GetTagsAsync(It.IsAny<string>())).Returns(Task.FromResult(tags));

            var controller = GetController(serviceMock);
            var result = await controller.GetTags();
            
            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(tags.Count, ((result as OkObjectResult).Value as List<TagModel>).Count);
        }

        [Fact(DisplayName = "Create Tags")]
        public async void CreateTags_Success()
        {
            var serviceMock = new Mock<ITagManagerService>(MockBehavior.Strict);
            var tags = TestDataHelper.CreateTags(2);
            serviceMock.Setup(m => m.CreateTagsAsync(It.IsAny<List<CreateTagModel>>(), It.IsAny<string>()))
                                    .Returns(Task.FromResult(tags.Count));

            var controller = GetController(serviceMock);
            var result = await controller.CreateTags(new List<CreateTagModel>());

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(tags.Count, (result as OkObjectResult).Value as int?);
        }

        [Fact(DisplayName = "Rename Tag")]
        public async void RenameTag_Success()
        {
            var serviceMock = new Mock<ITagManagerService>(MockBehavior.Strict);
            var tags = TestDataHelper.CreateTags(1);
            serviceMock.Setup(m => m.RenameTagAsync(It.IsAny<TagModel>(), It.IsAny<string>()))
                                    .Returns(Task.FromResult(tags[0]));

            var controller = GetController(serviceMock);
            var result = await controller.RenameTag(new TagModel());

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            Assert.Equal(tags[0].Name, ((result as OkObjectResult).Value as  TagModel).Name);
        }

        private TagManagerController GetController(Mock<ITagManagerService> serviceMock)
        {
            var controller = new TagManagerController(serviceMock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.HttpContext.Request.Headers.Authorization = TestDataHelper.GenerateJwtToken();
            return controller;
        }
    }
}
