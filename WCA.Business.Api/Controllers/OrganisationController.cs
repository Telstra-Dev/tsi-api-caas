using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Business.Api.Models;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace WCA.Business.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
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
        [ProducesResponseType(typeof(OrganisationViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetOrganisations([FromRoute] int customerId)
        {
            return Ok(this.service.GetOrganisation(customerId));
        }
    }
}
