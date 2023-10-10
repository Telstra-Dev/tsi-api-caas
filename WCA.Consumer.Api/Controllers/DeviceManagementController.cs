using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Helpers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("device-management")]
    public class DeviceManagementController : BaseController
    {
        readonly IDeviceManagementService _deviceManagementService;

        public DeviceManagementController(IDeviceManagementService deviceManagementService)
        {
            _deviceManagementService = deviceManagementService;
        }

        [HttpGet("rtsp-feed")]
        [ProducesResponseType(typeof(RtspFeedModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRtspFeed([FromQuery] string edgeDeviceId, [FromQuery] string leafDeviceId)
        {
            try
            {
                return Ok(await _deviceManagementService.GetRtspFeed(TokenHelper.GetToken(HttpContext), edgeDeviceId, leafDeviceId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
