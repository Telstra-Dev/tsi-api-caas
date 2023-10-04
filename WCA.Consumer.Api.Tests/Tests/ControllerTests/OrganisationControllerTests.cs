using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Controllers;
using Xunit;

namespace WCA.Customer.Api.Tests
{
    public class OrganisationControllerTests
    {
        [Fact(DisplayName = "Get organisation")]
        public async void GetOrganisation()
        {
            var serviceMock = new Mock<IOrganisationService>(MockBehavior.Strict);
            var myOrganisation = TestDataHelper.CreateOrganisationModel();
            serviceMock.Setup(m => m.GetOrganisation(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(myOrganisation));

            var controller = new OrganisationController(serviceMock.Object);

            var result = await controller.GetOrganisation(myOrganisation.CustomerId, false);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            OrganisationModel expectedOrganisation = ((result as OkObjectResult).Value as OrganisationModel);
            Assert.Equal(expectedOrganisation.CustomerId, myOrganisation?.CustomerId);
        }

        [Fact(DisplayName = "Get organisation overview")]
        public async void GetOrganisationOverview()
        {
            var serviceMock = new Mock<IOrganisationService>(MockBehavior.Strict);
            var myTenantOverview = TestDataHelper.CreateTenantOverview(1, 1, 1);
            serviceMock.Setup(m => m.GetOrganisationOverview(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(myTenantOverview));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer FAKE_TOKEN";

            var controller = new OrganisationController(serviceMock.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            var result = await controller.GetOrganisationOverview();

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            TenantOverview expectedTenantOverview = ((result as OkObjectResult).Value as TenantOverview);

            Assert.Equal(expectedTenantOverview.Sites.Count(), myTenantOverview?.Sites.Count());
            Assert.Equal(expectedTenantOverview.Sites.First().EdgeDevices.Count(), myTenantOverview?.Sites.First().EdgeDevices.Count());
        }

        [Fact(DisplayName = "Create organisation")]
        public async void CreateOrganisation()
        {
            var serviceMock = new Mock<IOrganisationService>(MockBehavior.Strict);
            var myOrganisation = TestDataHelper.CreateOrganisationModel();
            serviceMock.Setup(m => m.CreateOrganisation(It.IsAny<OrganisationModel>())).Returns(Task.FromResult(myOrganisation));

            var controller = new OrganisationController(serviceMock.Object);

            var result = await controller.CreateOrganisation(myOrganisation);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            OrganisationModel expectedOrganisation = ((result as OkObjectResult).Value as OrganisationModel);
            Assert.Equal(expectedOrganisation.CustomerId, myOrganisation?.CustomerId);
        }

        [Fact(DisplayName = "Update organisation")]
        public void UpdateOrganisation()
        {
            var serviceMock = new Mock<IOrganisationService>(MockBehavior.Strict);
            var myOrganisation = TestDataHelper.CreateOrganisationModel();
            serviceMock.Setup(m => m.UpdateOrganisation(It.IsAny<string>(), It.IsAny<OrganisationModel>())).Returns(myOrganisation);

            var controller = new OrganisationController(serviceMock.Object);

            var result = controller.UpdateOrganisation(myOrganisation.CustomerId, myOrganisation);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            OrganisationModel expectedOrganisation = ((result as OkObjectResult).Value as OrganisationModel);
            Assert.Equal(expectedOrganisation.CustomerId, myOrganisation?.CustomerId);
        }

        [Fact(DisplayName = "Delete organisation")]
        public void DeleteOrganisation()
        {
            var serviceMock = new Mock<IOrganisationService>(MockBehavior.Strict);
            var myOrganisation = TestDataHelper.CreateOrganisationModel();
            serviceMock.Setup(m => m.DeleteOrganisation(It.IsAny<string>())).Returns(myOrganisation);

            var controller = new OrganisationController(serviceMock.Object);

            var result = controller.DeleteOrganisation(myOrganisation.CustomerId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            OrganisationModel expectedOrganisation = ((result as OkObjectResult).Value as OrganisationModel);
            Assert.Equal(expectedOrganisation.CustomerId, myOrganisation?.CustomerId);
        }
    }
}
