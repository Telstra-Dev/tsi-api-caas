using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/api")]
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
        [ProducesResponseType(typeof(IList<Site>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetSites([FromQuery] string? customerId)
        {
            return Ok(this.service.GetSitesForCustomer(customerId));
        }

        /// <summary>
        /// Retrieves sites for given siteId
        /// </summary>
        /// <returns></returns>
        [HttpGet("sites/{siteId}")]
        [ProducesResponseType(typeof(IList<Site>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetSite([FromRoute] string? siteId)
        {
            return Ok(this.service.GetSite(siteId));
        }
    }
}
