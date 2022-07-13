using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Business.Api.Controllers
{
    [ApiController]
    [Route("/api")]
    public class OrganisationController : BaseController
    {
        readonly IOrganisationService service;

        public OrganisationController(IOrganisationService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Retrieves a singe organisation including any organisation hierarchy
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        [HttpGet("organisations/{customerId}")]
        [ProducesResponseType(typeof(Organisation), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetOrganisations([FromRoute] int customerId,
                                              [FromQuery] bool includeChildren = true,
                                              [FromQuery] bool displaySearchTree = true)
        {
            return Ok(this.service.GetOrganisation(customerId, includeChildren));
        }
    }
}
