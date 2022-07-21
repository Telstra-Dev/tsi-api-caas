using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

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
        [ProducesResponseType(typeof(Organisation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetOrganisations([FromRoute] int customerId,
                                              [FromQuery] bool includeChildren = true,
                                              [FromQuery] bool displaySearchTree = true)
        {
            return Ok(this._service.GetOrganisation(customerId, includeChildren));
        }

        /// <summary>
        /// Retrieve the search hierarchy for the logged-in user's organisation. Returns an array of nodes that can be turned 
        /// into a search tree. Used for presentation only.
        /// </summary>
        /// <returns></returns>
        [HttpGet("organisations/overview")]
        [ProducesResponseType(typeof(IList<Organisation>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganisationOverview()
        {
            return Ok(await _service.GetOrganisationOverview());
        }

        /// <summary>
        /// Creates a new organisation
        /// </summary>
        /// <returns></returns>
        [HttpPost("organisations")]
        [ProducesResponseType(typeof(Organisation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AllowAnonymous]
        public IActionResult CreateOrganisation([FromBody] Organisation org)
        {
            return Ok(_service.CreateOrganisation(org));
        }

        /// <summary>
        /// Updates an organisation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("organisations/{id}")]
        [ProducesResponseType(typeof(Organisation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult UpdateOrganisation([FromRoute] string id,
                                [FromBody] Organisation org)
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
