using System;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]s")]
    public class DeviceController : BaseController
    {
        readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService service)
        {
            _deviceService = service;
        }

        [HttpGet("edge-devices")]
        [ProducesResponseType(typeof(List<EdgeDeviceModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetEdgeDevices(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail
        )
        {
            try
            {
                return Ok(await _deviceService.GetEdgeDevices(authorisationEmail));
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("leaf-devices")]
        [ProducesResponseType(typeof(List<LeafDeviceModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLeafDevices(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail
        )
        {
            try
            {
                return Ok(await _deviceService.GetLeafDevices(authorisationEmail));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("edge-device/{deviceId}")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetEdgeDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string deviceId
        )
        {
            try
            {
                return Ok(await _deviceService.GetEdgeDevice(authorisationEmail, deviceId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("leaf-device/{deviceId}")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLeafDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string deviceId
        )
        {
            try
            {
                return Ok(await _deviceService.GetLeafDevice(authorisationEmail, deviceId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateEdgeDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] EdgeDeviceModel edgeDevice
        )
        {
            try
            {
                return Ok(await _deviceService.UpdateTsiEdgeDevice(authorisationEmail, edgeDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateLeafDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] LeafDeviceModel leafDevice
        )
        {
            try
            {
                return Ok(await _deviceService.UpdateLeafDevice(authorisationEmail, leafDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateEdgeDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] EdgeDeviceModel edgeDevice
        )
        {
            try
            {
                return Ok(await _deviceService.CreateEdgeDevice(authorisationEmail, edgeDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateLeafDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] LeafDeviceModel leafDevice
        )
        {
            try
            {
                return Ok(await _deviceService.CreateLeafDevice(authorisationEmail, leafDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteEdgeDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] EdgeDeviceModel edgeDevice
        )
        {
            try
            {
                return Ok(await _deviceService.DeleteEdgeDevice(authorisationEmail, edgeDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteLeafDevice(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] LeafDeviceModel leafDevice
        )
        {
            try
            {
                return Ok(await _deviceService.DeleteLeafDevice(authorisationEmail, leafDevice));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
