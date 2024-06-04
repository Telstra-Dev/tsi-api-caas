using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Telstra.Common;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Linq;
using System.Net.Http;
using System.Data;
using Flurl.Http;
using Flurl;
using System.Net;
using static WCA.Consumer.Api.Models.UMS.UmsOperationModels;
using System.Collections.Generic;
using WCA.Consumer.Api.Models.StorageResponse;
using WCA.Consumer.Api.Models.UMS;

namespace WCA.Consumer.Api.Services
{
    public class UserService : IUserService
    {
        private const string _AuthorizationHeader = "Authorization";
        private const string _emailNotFound = "User email not found";
        private const string _userNotFoundInTsiDb = "User not found in Tsi database";
        private const string _userNotFoundInUms = "User not found";
        private const string _emailAddressAlreadyInUse = "Email address already in use by another user.";
        private const string _noRoleSpecified = "At least one role must be specified";

        private readonly AppSettings _appSettings;
        private IUserManagementService _ums;
        private readonly ILogger _logger;

        public UserService(AppSettings appSettings,
                            IUserManagementService ums,
                            ILogger<UserService> logger)
        {
            _appSettings = appSettings;
            _ums = ums;
            _logger = logger;
        }

        public async Task<UserModel> GetUser(string userEmail, string accessToken)
        {
            try
            {
                var loadUmsUser = Task.Run
                    (
                        async () => await _ums.SearchByEmail(userEmail, accessToken)
                        ?? throw new Exception(_emailNotFound)
                    );

                var loadTsiUser = Task.Run
                    (
                        async () => await _GetUser(userEmail, accessToken)
                        ?? throw new Exception(_userNotFoundInTsiDb)
                    );

                Task.WaitAll(loadUmsUser, loadTsiUser);
                var umsUser = loadUmsUser.Result;
                var tsiUser = loadTsiUser.Result;

                return new UserModel
                {
                    Id = tsiUser.Id,
                    FirstName = tsiUser.FirstName,
                    LastName = tsiUser.LastName,
                    Nickname = tsiUser.Nickname,
                    Email = tsiUser.Email,
                    LastActive = DateTime.Now.ToString(),
                    Roles = umsUser.Roles.Where(r => r.Type.Split(":")[1].ToLower() == "tsi")
                                            .Select(role => role.Value.Split(":")[1]).ToArray(),
                    Sites = tsiUser.Sites.Select(site => site.SiteId).ToList()
                };
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"GetUser: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetUser: Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PermissionData>> FetchPermissions(string[] roles, string accessToken)
        {
            try
            {
                var roleNames = roles.Select(r => new UmsRole 
                {
                    RoleType = "tsi",
                    RoleName = r
                }).ToArray();

                var umsPermissions = await _ums.FetchRolePermissions(new UmsPermissionRequest
                {
                    RoleNames = roleNames
                }, accessToken);

                return umsPermissions.Data.Select(up => new PermissionData
                {
                    RoleType = up.RoleType,
                    RoleName = up.RoleName,
                    Permissions = up.Permissions
                }).ToList();
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"FetchPermissions: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FetchPermissions: Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<UmsRolePermissionResponseModel> FetchRolesAndPermissions(string accessToken)
        {
            try
            {
                return await _ums.FetchRolesAndPermissions(accessToken);
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"FetchRolesAndPermissions: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"FetchRolesAndPermissions: Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateUser(UserModel user, string authorisationEmail, string accessToken)
        {
            try
            {
                if (user.Roles == null || user.Roles.Count() == 0)
                {
                    throw new Exception(_noRoleSpecified);
                }

                var skipUmsUserCreation = false;
                UmsUser umsUser = await _ums.SearchByEmail(user.Email, accessToken);

                if (umsUser != null)
                {
                    try
                    {
                        if (await _GetUser(user.Email, accessToken) != null)
                        {
                            throw new Exception(_emailAddressAlreadyInUse);
                        }
                    }
                    catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        //The user exists in UMS but not added to TSI DB
                        //Let the user creation happen so that Tsi DB is updated for the user
                        skipUmsUserCreation = true;
                    }
                }

                var cidn = JwtExtractor.ExtractField(accessToken.Replace("Bearer ", string.Empty), "roles")
                                        .Split(":")[3].Replace("\"", "");
                UmsUser umsUserResponse = null;
                if (!skipUmsUserCreation)
                {
                    var newUser = new UmsAddUserRequest
                    {
                        UserName = user.Email,
                        GivenName = user.FirstName,
                        FamilyName = user.LastName,
                        ProductVerticalClientId = "b2b-tsi",
                        Roles = user.Roles.Select(role =>
                            new UmsUserRole
                            {
                                Type = "cidn:tsi",
                                Value = $"{cidn}:{role}"
                            }
                        ).ToList()
                    };
                    umsUserResponse = await _ums.CreateUser(newUser, accessToken);
                }

                if (skipUmsUserCreation || umsUserResponse != null)
                {
                    //Update the TsiUser site mappings
                    var response = await _appSettings.StorageAppHttp.BaseUri
                        .AppendPathSegment("users")
                        .AppendQueryParam("email", authorisationEmail)
                        .PostJsonAsync(user);
                    return await response.GetJsonAsync<bool>();
                }
                return false;
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"CreateUser: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateUser: Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateUser(UpdateUserModel user, string authorisationEmail, string accessToken)
        {
            try
            {
                UmsUser umsUser = await _ums.SearchByEmail(user.Email, accessToken);

                if (umsUser == null)
                {
                    throw new Exception(_userNotFoundInUms);
                }

                if (umsUser.UserName.ToLower() == authorisationEmail.ToLower())
                {
                    throw new UnauthorizedAccessException("User cannot update itself");
                }

                //Update user roles
                if (user.Roles != null && user.Roles.Any())
                {
                    var existingRoles = umsUser.Roles.Select(r => r.Value.Split(":")[1]);
                    var addRoles = user.Roles.Where(r => !existingRoles.Contains(r));
                    var removeRoles = existingRoles.Where(r => !user.Roles.Contains(r));

                    if (addRoles.Count() == 0 && removeRoles.Any() &&
                       removeRoles.Count() == existingRoles.Count() &&
                       removeRoles.All(r => { return existingRoles.Contains(r); }))
                    {
                        throw new Exception(_noRoleSpecified);
                    }

                    if (addRoles.Any())
                    {
                        await _ums.UpdateUser(new UmsUpdateUserRequest
                        {
                            Actions = "add",
                            Roles = addRoles.Select(role => new UmsUserRole
                            {
                                Type = "cidn:tsi",
                                Value = $"{umsUser.CIDN}:{role}"
                            }).ToList()
                        }, umsUser.Id, accessToken);
                    }

                    if (removeRoles.Any())
                    {
                        await _ums.UpdateUser(new UmsUpdateUserRequest
                        {
                            Actions = "remove",
                            Roles = removeRoles.Select(role => new UmsUserRole
                            {
                                Type = "cidn:tsi",
                                Value = $"{umsUser.CIDN}:{role}"
                            }).ToList()
                        }, umsUser.Id, accessToken);
                    }
                }

                //Update the TsiUser site mappings
                if (user.Sites != null && user.Sites.Count > 0)
                {
                    //read authorisation user roles
                    var authUser = await _ums.SearchByEmail(authorisationEmail, accessToken);
                    user.Roles = authUser.Roles.Where(r => r.Type.Split(":")[1].ToLower() == "tsi")
                                                .Select(role => role.Value.Split(":")[1]).ToArray();

                    var response = await _appSettings.StorageAppHttp.BaseUri
                        .AppendPathSegment("user")
                        .AppendQueryParam("email", authorisationEmail)
                        .SendJsonAsync(HttpMethod.Patch, user);
                    return await response.GetJsonAsync<bool>();
                }
                return true;
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"UpdateUser: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateUser: Exception: {ex.Message}");
                throw;
            }
        }

        private async Task<GetUserModel> _GetUser(string email, string accessToken)
        {
            try
            {
                var response = await $"{_appSettings.StorageAppHttp.BaseUri}"
                        .AppendPathSegment("user")
                        .AllowAnyHttpStatus()
                        .WithHeader(_AuthorizationHeader, accessToken)
                        .AppendQueryParam("email", email)
                        .OnError(call => { throw call.Exception; })
                        .GetAsync();

                if (response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException(_emailNotFound, null, HttpStatusCode.NotFound);
                }
                var tsiUser = await response.GetJsonAsync<GetUserModel>();
                return tsiUser;
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"_GetUser: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"_GetUser: Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<List<UserModel>> GetUsers(string userEmail, string accessToken)
        {
            try
            {
                var loadUmsUsers = Task.Run(async () => await _ums.GetAllUsers(accessToken));
                var loadTsiUsers = Task.Run(async () =>
                {
                    return await $"{_appSettings.StorageAppHttp.BaseUri}/users"
                                        .AllowAnyHttpStatus()
                                        .WithHeader(_AuthorizationHeader, accessToken)
                                        .AppendQueryParam("email", userEmail)
                                        .GetJsonAsync<List<UsersResponse>>();
                });

                Task.WaitAll(loadUmsUsers, loadUmsUsers);
                var umsUsers = loadUmsUsers.Result.UserData;
                var tsiUsers = loadTsiUsers.Result;

                return tsiUsers.Select(x => new UserModel
                {
                    Id = x.Id.ToString(),
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Nickname = x.NickName,
                    Email = x.Email,
                    LastActive = DateTime.Now.ToString(),
                    Roles = GetUserRoles(umsUsers, x.Email),
                    Sites = x.SiteUserMappings?.Select(y => y.SiteId).ToList()
                }).ToList();
            }
            catch (FlurlHttpException fhe)
            {
                var exceptionMessage = await fhe.GetResponseStringAsync();
                _logger.LogError($"GetUsers: Exception: {exceptionMessage}");
                throw new Exception(exceptionMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetUsers: Exception: {ex.Message}");
                throw;
            }
        }

        private string[] GetUserRoles(UserData[] umsUsers, string userEmail)
        {
            var currentUser = umsUsers.Where(x => x.Email == userEmail).FirstOrDefault();
            return currentUser?.Roles?.Select(x => x.Value.Split(":")[1]).ToArray();
        }
    }
}