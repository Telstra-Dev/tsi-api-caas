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
    public class SerialNumberControllerTests
    {
        public void GetSerialNumbers_Value_Success()
        {
            var serviceMock = new Mock<ISerialNumberService>(MockBehavior.Strict);
            var serialNumber = TestDataHelper.CreateSerialNumberModel();
            serviceMock.Setup(m => m.GetSerialNumberByValue(It.IsAny<string>())).Returns(Task.FromResult(serialNumber));

            var controller = new SerialNumberController(serviceMock.Object);
            var result = controller.GetSerialNumbers(serialNumber.Value, null);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            var expectedSerialNumber = ((result as OkObjectResult).Value as SerialNumberModel);
            Assert.Equal(expectedSerialNumber.SerialNumberId, serialNumber.SerialNumberId);
        }

        public void GetSerialNumbers_Value_NonExistent()
        {
            var serviceMock = new Mock<ISerialNumberService>(MockBehavior.Strict);
            SerialNumberModel serialNumber = null;
            serviceMock.Setup(m => m.GetSerialNumberByValue(It.IsAny<string>())).Returns(Task.FromResult(serialNumber));

            var controller = new SerialNumberController(serviceMock.Object);
            var result = controller.GetSerialNumbers("non-existent", null);

            Assert.Equal(typeof(NotFoundResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.NotFound, (result as NotFoundResult).StatusCode);
        }

        public void GetSerialNumbers_Filter_SingleMatch()
        {
            var serviceMock = new Mock<ISerialNumberService>(MockBehavior.Strict);
            var serialNumbers = TestDataHelper.CreateSerialNumberModels(1);
            var serialNumber = serialNumbers.First();
            serviceMock.Setup(m => m.GetSerialNumbersByFilter(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<uint?>()))
                .Returns(Task.FromResult(serialNumbers));

            var controller = new SerialNumberController(serviceMock.Object);
            var result = controller.GetSerialNumbers(null, serialNumber.Value);

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            var expectedSerialNumbers = (result as OkObjectResult).Value as List<SerialNumberModel>;
            Assert.Equal(expectedSerialNumbers.Count, 1);
            Assert.Equal(expectedSerialNumbers[0].SerialNumberId, serialNumber.SerialNumberId);
            Assert.Equal(expectedSerialNumbers[0].Value, serialNumber.Value);
            Assert.Equal(expectedSerialNumbers[0].DeviceId, serialNumber.DeviceId);
        }

        public void GetSerialNumbers_Filter_MultipleMatches()
        {
            var serviceMock = new Mock<ISerialNumberService>(MockBehavior.Strict);
            var count = 5;
            var serialNumbers = TestDataHelper.CreateSerialNumberModels(1);
            var serialNumber = serialNumbers.First();
            serviceMock.Setup(m => m.GetSerialNumbersByFilter(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<uint?>()))
                .Returns(Task.FromResult(serialNumbers));

            var controller = new SerialNumberController(serviceMock.Object);
            var result = controller.GetSerialNumbers(null, serialNumber.Value.Substring(0, 10));

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            var expectedSerialNumbers = (result as OkObjectResult).Value as List<SerialNumberModel>;
            Assert.Equal(expectedSerialNumbers.Count, count);
            for (int i = 0; i < serialNumbers.Count; i++)
            {
                Assert.Equal(expectedSerialNumbers[i].SerialNumberId, serialNumbers[i].SerialNumberId);
                Assert.Equal(expectedSerialNumbers[i].Value, serialNumbers[i].Value);
                Assert.Equal(expectedSerialNumbers[i].DeviceId, serialNumbers[i].DeviceId);
            }
        }

        public void GetSerialNumbers_Filter_NonExistent()
        {
            var serviceMock = new Mock<ISerialNumberService>(MockBehavior.Strict);
            serviceMock.Setup(m => m.GetSerialNumbersByFilter(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<uint?>()))
                .Returns(Task.FromResult<IList<SerialNumberModel>>(null));

            var controller = new SerialNumberController(serviceMock.Object);
            var result = controller.GetSerialNumbers(null, "non-existent");

            Assert.Equal(typeof(OkObjectResult), result.GetType());
            Assert.Equal((int)HttpStatusCode.OK, (result as OkObjectResult).StatusCode);
            var expectedSerialNumbers = (result as OkObjectResult).Value as List<SerialNumberModel>;
            Assert.Equal(expectedSerialNumbers.Count, 0);
        }
    }
}
