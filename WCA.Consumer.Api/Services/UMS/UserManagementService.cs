using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telstra.Common;
using System;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Flurl.Http;
using Flurl;
using static WCA.Consumer.Api.Models.UMS.UmsOperationModels;
using System.Text.Json;
using WCA.Consumer.Api.Models.UMS;

namespace WCA.Consumer.Api.Services.UMS
{
    public class UserManagementService : IUserManagementService
    {
        private readonly string _errNoAccessToken = "Access token cannot be empty";
        private readonly string _cache_token_key = "ums_token_key";

        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        public UserManagementService(AppSettings appSettings,
                                     IMemoryCache cache,
                                     ILogger<UserManagementService> logger)
        {
            _appSettings = appSettings;
            _cache = cache;
            _logger = logger;
            _shortCacheTime = DateTimeOffset.Now.AddSeconds(_appSettings.ShortCacheTime);
        }

        public async Task<UmsUser> SearchByEmail(string email, string accessToken)
        {
            try
            {
                var response = await $"{_appSettings.UserManagementService.Endpoint}"
                    .AppendPathSegment("users/search")
                    .WithOAuthBearerToken(GetBearerToken(accessToken))
                    .PostJsonAsync(new { userName = email });
                var users = await response.GetJsonAsync<UmsUsers>();
                return users.Data.FirstOrDefault();
            }
            catch (FlurlHttpException ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        public async Task<UmsPermissionResponse> FetchRolePermissions(UmsPermissionRequest request, string accessToken)
        {
            var response = await _appSettings.UserManagementService.Endpoint
                .AppendPathSegment("roles/fetch-permissions")
                .WithOAuthBearerToken(GetBearerToken(accessToken))
                .PostJsonAsync(request);
            return await response.GetJsonAsync<UmsPermissionResponse>();
        }

        public async Task<UmsRolePermissionResponseModel> FetchRolesAndPermissions(string accessToken)
        {
            return await _cache.GetOrCreateAsync("TsiRolesPermissions", async entry =>
            {
                var bearerToken = GetBearerToken(accessToken);
                var rolesResponse = await _appSettings.UserManagementService.Endpoint
                                .AppendPathSegment("roles")
                                .AppendQueryParam("pv_client_id", "b2b-tsi")
                                .WithOAuthBearerToken(bearerToken)
                                .GetStringAsync();
                var rolesJson = JsonDocument.Parse(rolesResponse);
                var roleElements = rolesJson.RootElement.GetProperty("data")[0].GetProperty("roleNames");
                var tsiRoles = roleElements.EnumerateArray().Select(role => role.GetString()).ToList();
                var rolePermissionsRequestModel = new UmsRolePermissionRequestModel
                {
                    role_names = tsiRoles.Select(x => new Role_Names
                    {
                        roleName = x,
                        roleType = "tsi"
                    }).ToArray()
                };
                var permissionsResponse = await _appSettings.UserManagementService.Endpoint
                                                    .AppendPathSegment("roles/fetch-permissions")
                                                    .WithOAuthBearerToken(bearerToken)
                                                    .PostJsonAsync(rolePermissionsRequestModel);
                entry.SetAbsoluteExpiration(_shortCacheTime);
                return await permissionsResponse.GetJsonAsync<UmsRolePermissionResponseModel>();
            });
        }

        public async Task<UmsUser> CreateUser(UmsAddUserRequest request, string accessToken)
        {
            var response = await _appSettings.UserManagementService.Endpoint
                .AppendPathSegment("users")
                .WithOAuthBearerToken(GetBearerToken(accessToken))
                .PostJsonAsync(request);
            return await response.GetJsonAsync<UmsUser>();
        }

        public async Task<UmsUser> UpdateUser(UmsUpdateUserRequest request, string userId, string accessToken)
        {
            var response = await _appSettings.UserManagementService.Endpoint
               .AppendPathSegment($"users/{userId}")
               .WithOAuthBearerToken(GetBearerToken(accessToken))
               .PatchJsonAsync(request);
            return await response.GetJsonAsync<UmsUser>();
        }

        public async Task<UmsUsersResponseModel> GetAllUsers(string accessToken)
        {
            var bearerToken = GetBearerToken(accessToken);
            var cidn = JwtExtractor.ExtractField(bearerToken, "roles")
                                        .Split(":")[3].Replace("\"", "");
            var clientId = JwtExtractor.ExtractField(bearerToken, "aud");
            return await _appSettings.UserManagementService.Endpoint
                                    .AppendPathSegment("users")
                                    .AppendQueryParam("cidn", cidn)
                                    .AppendQueryParam("pv_client_id", clientId)
                                    .WithOAuthBearerToken(bearerToken)
                                    .GetJsonAsync<UmsUsersResponseModel>();
        }

        private string GetBearerToken(string auth = null)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(auth)
                    ? auth.Replace("Bearer ", string.Empty)
                    : throw new Exception(_errNoAccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAccessToken (UMS): " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
