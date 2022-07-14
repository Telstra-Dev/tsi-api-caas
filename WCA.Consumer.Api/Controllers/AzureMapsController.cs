using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telstra.Core.Contracts;
using Telstra.Core.Api.Helpers;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Produces("application/json")]
    public class AzureMapsController : BaseController
    {
        readonly IAzureMapsAuthService service;

        public AzureMapsController(IAzureMapsAuthService service)
        {
            this.service = service;
        }

        [AllowAnonymous]
        [HttpGet("/azure-maps/oauth2/token")]
        public async Task<IActionResult> GetAuthToken()
        {

            // @TODO : Prior to this we NEED to be doing some authorisation

            WCAHttpClient httpClient = new WCAHttpClient();
            var responseText = await httpClient.GetAzureMapsAuthToken();
            return Ok(responseText);
        }
    }
}
