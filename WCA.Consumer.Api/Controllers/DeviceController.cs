using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]s")]
    public class DeviceController : BaseController
    {
        readonly IDeviceService service;

        public DeviceController(IDeviceService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Devices come in multiple types. Edge capable devices like gateways, and end devices like cameras, sensors.
        /// Query by deviceId or siteId or customerId. Not sending any of them will return device for logged-in customer.
        /// When sending more than one query param, params with high specificity will be given priority.
        /// Example: When sending deviceId and siteId together, siteId will be ignored.
        /// Example: When sending siteId and customerId together, customerId will be ignored.
        /// Logged-in customer must own the devices directly or indirectly, otherwise 401 will be returned.
        /// Returns one or more Devices.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(ArrayList), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetDevices([FromQuery] string customerId = null,
                                        [FromQuery] string siteId = null)
        {
            return Ok(this.service.GetDevices(customerId, siteId));
        }

        [HttpGet("{deviceId}")]
        [ProducesResponseType(typeof(DeviceBase), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetDevice([FromRoute] string deviceId)
        {
            return Ok(this.service.GetDevice(deviceId));
        }

        /// <summary>
        /// Creates a new camera device
        /// </summary>
        /// <returns></returns>
        [HttpPost("camera")]
        [ProducesResponseType(typeof(Camera), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public IActionResult CreateCameraDevice([FromBody] Camera device)
        {
            return Ok(this.service.CreateCameraDevice(device));
        }

        /// <summary>
        /// Creates a new gateway device
        /// </summary>
        /// <returns></returns>
        [HttpPost("edge-device")]
        [ProducesResponseType(typeof(Gateway), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public IActionResult CreateEdgeDevice([FromBody] Gateway device)
        {
            return Ok(this.service.CreateEdgeDevice(device));
        }

        /// <summary>
        /// Updates a camera device
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpPut("camera/{deviceId}")]
        [ProducesResponseType(typeof(Camera), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateCameraDevice([FromRoute] string deviceId,
                                [FromBody] Camera device)
        {
            return Ok(this.service.UpdateCameraDevice(deviceId, device));
        }

        /// <summary>
        /// Updates a gateway device
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpPut("edge-device/{deviceId}")]
        [ProducesResponseType(typeof(Gateway), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateEdgeDevice([FromRoute] string deviceId,
                                [FromBody] Gateway device)
        {
            return Ok(this.service.UpdateEdgeDevice(deviceId, device));
        }

    }
}
