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
        public async Task<IActionResult> GetSites([FromQuery] string customerId)
        {
            try
            {
                var newSite = await _siteService.GetSitesForCustomer(customerId);
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
        [HttpGet("sites/{siteId}")]
        [ProducesResponseType(typeof(IList<SiteModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSite([FromRoute] string siteId, [FromQuery] string customerId)
        {
            try
            {
                var newSite = await _siteService.GetSite(siteId, customerId);
                if (newSite != null)
                    return Ok(newSite);
                else
                    return NotFound(new { message = "Site doesn't exist with this customer" });
            }
            catch (Exception e)
            {
                return BadRequest(e);
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
        public async Task<IActionResult> CreateSite([FromBody] SiteModel site)
        {
            try
            {
                var newSite = await _siteService.CreateSite(site);

                return Ok(newSite);
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
        public async Task<IActionResult> UpdateSite([FromRoute] string siteId,
                                [FromBody] SiteModel site)
        {
            try
            {
                if (site.SiteId == null)
                {
                    site.SiteId = siteId;
                }
                var updatedSite = await _siteService.UpdateSite(siteId, site);

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
        public async Task<IActionResult> DeleteSite([FromRoute] string siteId)
        {
            try
            {
                var updatedSite = await _siteService.DeleteSite(siteId);

                return Ok(updatedSite);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
