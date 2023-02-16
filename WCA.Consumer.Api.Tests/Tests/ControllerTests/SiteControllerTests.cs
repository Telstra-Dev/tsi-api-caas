using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class SiteControllerTests
    {
        [Fact(DisplayName = "Get sites")]
        public void GetSites_Success()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var mySites = TestDataHelper.CreateSiteModels(1);
            serviceMock.Setup(m => m.GetSitesForCustomer(It.IsAny<string>())).Returns(Task.FromResult(mySites));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.GetSites(mySites.First().CustomerId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            IList<SiteModel> expectedSites = ((result as OkObjectResult).Value as IList<SiteModel>);
            var expectedSite = (SiteModel) expectedSites[0];
            Assert.Equal(expectedSite.SiteId, mySites.First().SiteId);
        }

        [Fact(DisplayName = "Get sites not found")]
        public void GetSites()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var customerId = "customer-id";
            serviceMock.Setup(m => m.GetSitesForCustomer(It.IsAny<string>())).Returns(Task.FromResult<IList<SiteModel>>(null));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.GetSites(customerId);

            Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundObjectResult).StatusCode);
        }

        [Fact(DisplayName = "Get one site")]
        public void GetSite_Success()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var mySite = TestDataHelper.CreateSiteModel();
            serviceMock.Setup(m => m.GetSite(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(mySite));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.GetSite(mySite.SiteId, mySite.CustomerId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            SiteModel expectedSite = ((result as OkObjectResult).Value as SiteModel);
            Assert.Equal(expectedSite.SiteId, mySite?.SiteId);
        }

        [Fact(DisplayName = "Get site not found")]
        public void GetSite_NotFound()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var customerId = "customer-id";
            var siteId = "site-id";
            serviceMock.Setup(m => m.GetSite(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<SiteModel>(null));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.GetSite(siteId, customerId);

            Assert.Equal(typeof(NotFoundObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundObjectResult).StatusCode);
        }

        [Fact(DisplayName = "Create site")]
        public void CreateSite()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var mySite = TestDataHelper.CreateSiteModel();
            serviceMock.Setup(m => m.CreateSite(It.IsAny<SiteModel>())).Returns(Task.FromResult(mySite));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.CreateSite(mySite);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            SiteModel expectedSite = ((result as OkObjectResult).Value as SiteModel);
            Assert.Equal(expectedSite.SiteId, mySite?.SiteId);
        }

        [Fact(DisplayName = "Update site")]
        public void UpdateSite()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var mySite = TestDataHelper.CreateSiteModel();
            serviceMock.Setup(m => m.UpdateSite(It.IsAny<string>(), It.IsAny<SiteModel>())).Returns(Task.FromResult(mySite));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.UpdateSite(mySite.SiteId, mySite);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            SiteModel expectedSite = ((result as OkObjectResult).Value as SiteModel);
            Assert.Equal(expectedSite.SiteId, mySite?.SiteId);
        }

        [Fact(DisplayName = "Delete site")]
        public void DeleteSite()
        {
            var serviceMock = new Mock<ISiteService>(MockBehavior.Strict);
            var mySite = TestDataHelper.CreateSiteModel();
            serviceMock.Setup(m => m.DeleteSite(It.IsAny<string>())).Returns(Task.FromResult(mySite));

            var controller = new SiteController(serviceMock.Object);

            var result = controller.DeleteSite(mySite.SiteId);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            SiteModel expectedSite = ((result as OkObjectResult).Value as SiteModel);
            Assert.Equal(expectedSite.SiteId, mySite?.SiteId);
        }
    }
}
