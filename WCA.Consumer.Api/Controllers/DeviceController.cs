using System;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading.Tasks;
using System.Collections.Generic;
using WCA.Consumer.Api.Helpers;

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
        public async Task<IActionResult> GetEdgeDevices()
        {
            try
            {
                return Ok(await _deviceService.GetEdgeDevices(TokenHelper.GetToken(HttpContext)));
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
        public async Task<IActionResult> GetLeafDevices()
        {
            try
            {
                return Ok(await _deviceService.GetLeafDevices(TokenHelper.GetToken(HttpContext)));
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
        public async Task<IActionResult> GetEdgeDevice([FromRoute] string deviceId)
        {
            try
            {
                return Ok(await _deviceService.GetEdgeDevice(deviceId, TokenHelper.GetToken(HttpContext)));
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
        public async Task<IActionResult> GetLeafDevice([FromRoute] string deviceId)
        {
            try
            {
                return Ok(await _deviceService.GetLeafDevice(deviceId, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateEdgeDevice([FromBody] EdgeDeviceModel edgeDevice)
        {
            try
            {
                return Ok(await _deviceService.UpdateTsiEdgeDevice(edgeDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateLeafDevice([FromBody] LeafDeviceModel leafDevice)
        {
            try
            {
                return Ok(await _deviceService.UpdateLeafDevice(leafDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateEdgeDevice([FromBody] EdgeDeviceModel edgeDevice)
        {
            try
            {
                return Ok(await _deviceService.CreateEdgeDevice(edgeDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateLeafDevice([FromBody] LeafDeviceModel leafDevice)
        {
            try
            {
                return Ok(await _deviceService.CreateLeafDevice(leafDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("edge-device")]
        [ProducesResponseType(typeof(EdgeDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteEdgeDevice([FromBody] EdgeDeviceModel edgeDevice)
        {
            try
            {
                return Ok(await _deviceService.DeleteEdgeDevice(edgeDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("leaf-device")]
        [ProducesResponseType(typeof(LeafDeviceModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteLeafDevice([FromBody] LeafDeviceModel leafDevice)
        {
            try
            {
                return Ok(await _deviceService.DeleteLeafDevice(leafDevice, TokenHelper.GetToken(HttpContext)));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
