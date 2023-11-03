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
    public class SiteController : BaseController
    {
        readonly ISiteService _siteService;

        public SiteController(ISiteService siteService)
        {
            _siteService = siteService;
        }

        /// <summary>
        /// Retrieves sites specified customerId or for logged in customer if no customerId supplied. 
        /// Logged in customer must own the site otherwise 401 will be returned.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("sites")]
        [ProducesResponseType(typeof(IList<SiteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSites(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string customerId
        )
        {
            try
            {
                if (string.IsNullOrEmpty(customerId))
                {
                    //data from flexi DB
                    var sites = await _siteService.GetSites(authorisationEmail);

                    return Ok(sites);
                }
                else
                {
                    // TODO (@Jason): check the following path - this doesn't seem to make sense from authorisation perspective; we should remove once integration points are confirmed.
                    var newSite = await _siteService.GetSitesForCustomer(authorisationEmail, customerId);
                    if (newSite?.Count > 0)
                        return Ok(newSite);
                    else
                        return NotFound(new { message = "No sites exist with this customer" });
                }
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
        [HttpGet("sites/{siteId}")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string siteId,
            [FromQuery] bool telemetryProperties = false
        )
        {
            try
            {
                if (telemetryProperties)
                {   
                    var result = await _siteService.GetSiteTelProperties(authorisationEmail, siteId);
                    return Ok(result);
                }
                else
                {
                    return Ok(await _siteService.GetSiteById(authorisationEmail, siteId));
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
        [HttpPost("sites")]
        [ProducesResponseType(typeof(SiteModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] SiteModel site
        )
        {
            try
            {
                return Ok(await _siteService.CreateSite(authorisationEmail, site));
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        /// <summary>
        /// Updates a site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpPut("sites/{siteId}")]
        [ProducesResponseType(typeof(SiteModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateSite(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string siteId,
            [FromBody] SiteModel site
        )
        {
            try
            {
                if (site.SiteId == null)
                {
                    site.SiteId = siteId;
                }
                var updatedSite = await _siteService.UpdateSite(authorisationEmail, siteId, site);

                return Ok(updatedSite);
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
                var updatedSite = await _siteService.DeleteSite(authorisationEmail, siteId);

                return Ok(updatedSite);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
