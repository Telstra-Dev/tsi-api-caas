using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class SiteController(ISiteService siteService) : BaseController
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
    }
}
