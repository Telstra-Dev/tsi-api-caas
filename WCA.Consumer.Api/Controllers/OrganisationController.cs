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
        readonly IOrganisationService _service;

        public OrganisationController(IOrganisationService service)
        {
            this._service = service;
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
        public IActionResult GetOrganisation([FromRoute] string customerId,
                                              [FromQuery] bool includeChildren = true,
                                              [FromQuery] bool displaySearchTree = true)
        {
            return Ok(this._service.GetOrganisation(customerId, includeChildren).Result);
        }

        /// <summary>
        /// Retrieve the search hierarchy for the logged-in user's organisation. Returns an array of nodes that can be turned 
        /// into a search tree. Used for presentation only.
        /// </summary>
        /// <returns></returns>
        [HttpGet("organisations/overview")]
        [ProducesResponseType(typeof(IList<OrganisationModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganisationOverview()
        {
            try
            {
                return Ok(await _service.GetOrganisationOverview());
            }
            catch (Exception e) 
            {
                if (e is ArgumentOutOfRangeException) {
                    return new BadRequestObjectResult(e.Message);
                }
                throw new Exception(e.Message);;
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
        public async Task<IActionResult> CreateOrganisation([FromBody] OrganisationModel org)
        {
            try
            {
                return Ok(await _service.CreateOrganisation(org));
            }
            catch (Exception e) 
            {
                if (e is ArgumentOutOfRangeException) {
                    return new BadRequestObjectResult(e.Message);
                }
                throw new Exception(e.Message);
            }
            //return Ok(_service.CreateOrganisation(org));
        }

        /// <summary>
        /// Updates an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("organisations/{id}")]
        [ProducesResponseType(typeof(OrganisationModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateOrganisation([FromRoute] string id,
                                [FromBody] OrganisationModel org)
        {
            return Ok(_service.UpdateOrganisation(id, org));
        }

        /// <summary>
        /// Deletes an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("organisations/{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult DeleteOrganisation([FromRoute] string id)
        {
            return Ok(_service.DeleteOrganisation(id));
        }
    }
}
