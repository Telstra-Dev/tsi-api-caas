using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class SiteController(ISiteService siteService, ILogger<SiteController> _logger) : BaseController
    {
        [HttpGet("/sites")]
        [ProducesResponseType(typeof(IList<SiteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSites(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail
        )
        {
            try
            {
                var newSite = await siteService.GetSites(authorisationEmail);
                if (newSite?.Count > 0)
                    return Ok(newSite);
                else
                    return NotFound(new { message = "No sites exist with this customer" });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("/sites/group-by-tags")]
        [ProducesResponseType(typeof(IList<SiteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSitesGroupedByTags(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authorisationEmail))
                {
                    return BadRequest($"[ValidationError] No authorisationEmail specified.");
                }

                var result = await siteService.GetSitesGroupedByTags(authorisationEmail);
                if (result?.Count > 0)
                    return Ok(result);
                else
                    return NotFound(new { message = "No sites exist with this customer" });
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Retrieves sites for given siteId
        /// </summary>
        /// <returns></returns>
        [HttpGet("/sites/{siteId:int}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] int siteId,
            [FromQuery] bool telemetryProperties = false
        )
        {
            try
            {
                if (telemetryProperties)
                {   
                    var result = await siteService.GetSiteTelProperties(authorisationEmail, siteId);
                    return Ok(result);
                }
                else
                {
                    return Ok(await siteService.GetSiteById(authorisationEmail, siteId));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Creates a new site
        /// </summary>
        /// <returns></returns>
        [HttpPost("/sites")]
        [HttpPut("/sites/{siteId:int}")]
        [ProducesResponseType(typeof(SiteModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] SiteModel site,
            [FromRoute] int? siteId = null
        )
        {
            try
            {
                site.SiteId = siteId ?? 0;
                var id = await siteService.CreateOrUpdateSite(authorisationEmail, site);
                if (id > 0)
                    return await GetSite(authorisationEmail, id, false);

                return Problem("Site Creation Failed");
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }


        /// <summary>
        /// Delete a site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpDelete("sites/{siteId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeleteSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string siteId
        )
        {
            try
            {
                var updatedSite = await siteService.DeleteSite(authorisationEmail, siteId);

                return Ok(updatedSite);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost("sites/location")]
        [ProducesResponseType(typeof(List<TagModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLocationTelemetryForSites([FromHeader(Name = "X-CUsername")] string authorisationEmail, [FromBody] List<int> siteIds)
        {
            if (siteIds == null || !siteIds.Any())
            {
                return BadRequest("Site IDs cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                return BadRequest($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var result = await siteService.GetLocationTelemetry(authorisationEmail, siteIds);

                if (result == null || !result.Any())
                {
                    return NotFound("No telemetry data found for the given site IDs.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
