using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Helpers;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceManagementController : BaseController
    {
        readonly IDeviceManagementService _deviceManagementService;

        public DeviceManagementController(IDeviceManagementService deviceManagementService)
        {
            _deviceManagementService = deviceManagementService;
        }

        [HttpGet("rtspFeed")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRtspFeed([FromQuery] string deviceId, [FromQuery] string moduleId)
        {
            try
            {
                return Ok(await _deviceManagementService.GetRtspFeed(TokenHelper.GetToken(HttpContext), deviceId, moduleId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
