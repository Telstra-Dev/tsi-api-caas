using System;
using Microsoft.AspNetCore.Mvc;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using WCA.Consumer.Api.Services;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : BaseController
    {
        readonly IUserService _userService;
        readonly IUserManagementService _ums;
        readonly ILogger<UserController> _logger;

        public UserController(IUserService siteService,
                                IUserManagementService ums,
                                ILogger<UserController> logger)
        {
            _userService = siteService;
            _ums = ums;
            _logger = logger;
        }

        [HttpGet("[controller]/role-permissions")]
        public async Task<IActionResult> GetRolePermissions(
            [FromHeader(Name = "Authorization")] string auth,
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string role,
            [FromQuery] string roleType = "tsi")
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                var permissions = await _userService.FetchPermissions([role], auth);
                return Ok(permissions);
            }
            catch (Exception e)
            {
                _logger.LogError($"GetRolePermissions: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[controller]/permissions")]
        public async Task<IActionResult> GetUserPermissions(
            [FromHeader(Name = "Authorization")] string auth,
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string email)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                var umsUser = await _ums.SearchByEmail(email, auth);
                var userRoles = umsUser.Roles.Where(r => r.Type.Split(":")[1].ToLower() == "tsi")
                                                .Select(role => role.Value.Split(":")[1]).ToArray();
                return Ok(await _userService.FetchPermissions(userRoles, auth));
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUserPermissions: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[controller]")]
        public async Task<IActionResult> GetUser(
            [FromHeader(Name = "Authorization")] string auth,
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string email)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                var user = await _userService.GetUser(email, auth);
                return Ok(user);
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUser: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[controller]s")]
        public async Task<IActionResult> GetUsers(
            [FromHeader(Name = "Authorization")] string auth,
            [FromHeader(Name = "X-CUsername")] string authorisationEmail)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                return Ok(await _userService.GetUsers(authorisationEmail, auth));
            }
            catch (Exception e)
            {
                _logger.LogError($"GetUsers: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[controller]/roles-permissions")]
        public async Task<IActionResult> GetRolesAndPermissions(
            [FromHeader(Name = "Authorization")] string auth,
            [FromHeader(Name = "X-CUsername")] string authorisationEmail)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                return Ok(await _userService.FetchRolesAndPermissions(auth));
            }
            catch (Exception e)
            {
                _logger.LogError($"GetRolesAndPermissions: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[controller]s")]
        public async Task<IActionResult> CreateUser(
             [FromHeader(Name = "Authorization")] string auth,
             [FromHeader(Name = "X-CUsername")] string authorisationEmail,
             [FromBody] UserModel user)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                var response = await _userService.CreateUser(user, authorisationEmail, auth);
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"CreateUser: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        [HttpPatch("[controller]")]
        public async Task<IActionResult> UpdateUser(
             [FromHeader(Name = "Authorization")] string auth,
             [FromHeader(Name = "X-CUsername")] string authorisationEmail,
             [FromBody] UpdateUserModel user)
        {
            try
            {
                ValidateHeaders(auth, authorisationEmail);
                var response = await _userService.UpdateUser(user, authorisationEmail, auth);
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError($"UpdateUser: Error: {e.Message}");
                return BadRequest(e.Message);
            }
        }

        // Validate incoming headers
        private void ValidateHeaders(string auth, string authorisationEmail)
        {
            ArgumentNullException.ThrowIfNull(auth);

            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }
        }
    }
}
