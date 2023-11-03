using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class HealthStatusController : BaseController
    {
        readonly IHealthStatusService service;

        public HealthStatusController(IHealthStatusService service)
        {
            this.service = service;
        }

        [HttpGet("healthStatus")]
        [ProducesResponseType(typeof(IList<HealthStatusModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetHealthStatus(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string deviceId,
            [FromQuery] string siteId
        )
        {
            try {
                if (deviceId != null)
                {
                    var deviceHealth = await this.service.GetHealthStatusFromDeviceId(authorisationEmail, deviceId);
                    if (deviceHealth != null)
                        return Ok(deviceHealth);
                    else
                        return NotFound(new { message = "Device health not found" });
                }
                else
                {
                    var siteHealth = await this.service.GetHealthStatusFromSiteId(authorisationEmail, siteId);
                    if (siteHealth != null)
                        return Ok(siteHealth);
                    else
                        return NotFound(new { message = "Site health not found" });
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
