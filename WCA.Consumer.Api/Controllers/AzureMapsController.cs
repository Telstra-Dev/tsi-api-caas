using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telstra.Core.Contracts;
using Telstra.Common;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/azure-maps")]
    public class AzureMapsController : BaseController
    {
        readonly IAzureMapsAuthService _service;
        private readonly AppSettings _appSettings;

        public AzureMapsController(IAzureMapsAuthService service, AppSettings appSettings)
        {
            this._service = service;
            this._appSettings = appSettings;
        }

        [AllowAnonymous]
        [HttpGet("/oauth2/token")]
        public async Task<IActionResult> GetAuthToken()
        {

            // @TODO : Prior to this we NEED to be doing some authorisation

            return Ok(
                await _service.GetAuthToken(
                    _appSettings.AzureMapsAuthCredentials.AuthUri,
                    _appSettings.AzureMapsAuthCredentials.ClientId,
                    _appSettings.AzureMapsAuthCredentials.ClientSecret,
                    _appSettings.AzureMapsAuthCredentials.GrantType,
                    _appSettings.AzureMapsAuthCredentials.Resource
                ));
        }
    }
}
