using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    public class OrganisationController : BaseController
    {
        readonly IOrganisationService _orgService;

        public OrganisationController(IOrganisationService orgService)
        {
            _orgService = orgService;
        }

        /// <summary>
        /// Retrieves a single organisation including any organisation hierarchy
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("organisation/{customerId}")]
        [ProducesResponseType(typeof(OrganisationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganisation(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string customerId,
            [FromQuery] bool includeChildren = true,
            [FromQuery] bool displaySearchTree = true
        )
        {
            // TODO (@Jason): check the following path - this doesn't seem to make sense from authorisation perspective; we should remove once integration points are confirmed.
            return Ok(await _orgService.GetOrganisation(authorisationEmail, customerId, includeChildren));
        }

        /// <summary>
        /// Retrieve the search hierarchy for the logged-in user's organisation. Returns an array of nodes that can be turned 
        /// into a search tree. Used for presentation only.
        /// </summary>
        /// <returns></returns>
        [HttpGet("organisations/overview")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganisationOverview(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] bool includeHealthStatus = false
        )
        {
            try
            {
                return Ok(await _orgService.GetOrganisationOverview(authorisationEmail, includeHealthStatus));
            }
            catch (NullReferenceException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Creates a new organisation
        /// </summary>
        /// <returns></returns>
        [HttpPost("organisations")]
        [ProducesResponseType(typeof(OrganisationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrganisation(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] OrganisationModel org
        )
        {
            try
            {
                return Ok(await _orgService.CreateOrganisation(authorisationEmail, org));
            }
            catch (Exception e)
            {
                if (e is ArgumentOutOfRangeException)
                {
                    return new BadRequestObjectResult(e.Message);
                }
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Updates an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("organisations/{id}")]
        [ProducesResponseType(typeof(OrganisationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateOrganisation(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string id,
            [FromBody] OrganisationModel org
        )
        {
            return Ok(_orgService.UpdateOrganisation(authorisationEmail, id, org));
        }

        /// <summary>
        /// Deletes an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("organisations/{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult DeleteOrganisation(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromRoute] string id
        )
        {
            return Ok(_orgService.DeleteOrganisation(authorisationEmail, id));
        }
    }
}
