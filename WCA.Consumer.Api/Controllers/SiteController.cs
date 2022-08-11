using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class SiteController : BaseController
    {
        readonly ISiteService service;

        public SiteController(ISiteService service)
        {
            this.service = service;
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
        public IActionResult GetSites([FromQuery] string customerId)
        {
            return Ok(this.service.GetSitesForCustomer(customerId).Result);
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
        public IActionResult GetSite([FromRoute] string siteId)
        {
            return Ok(this.service.GetSite(siteId));
        }

        /// <summary>
        /// Creates a new site
        /// </summary>
        /// <returns></returns>
        [HttpPost("sites")]
        [ProducesResponseType(typeof(SiteModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public IActionResult CreateSite([FromBody] SiteModel site)
        {
            try {
                var newSite = this.service.CreateSite(site).Result;

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
        public IActionResult UpdateSite([FromRoute] string siteId,
                                [FromBody] SiteModel site)
        {
            return Ok(this.service.UpdateSite(siteId, site));
        }

        /// <summary>
        /// Delete a site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [HttpDelete("sites/{siteId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult DeleteSite([FromRoute] string siteId)
        {
            return Ok(this.service.DeleteSite(siteId).Result);
        }
    }
}
