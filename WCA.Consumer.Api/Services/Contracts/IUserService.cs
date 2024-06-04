using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Models.UMS;
using static WCA.Consumer.Api.Models.UMS.UmsOperationModels;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IUserService
    {
        Task<bool> CreateUser(UserModel user, string authorisationEmail, string accessToken);
        Task<bool> UpdateUser(UpdateUserModel user, string authorisationEmail, string accessToken);
        Task<UserModel> GetUser(string userEmail, string accessToken);
        Task<List<UserModel>> GetUsers(string userEmail, string accessToken);
        Task<List<PermissionData>> FetchPermissions(string[] roles, string accessToken);
        Task<UmsRolePermissionResponseModel> FetchRolesAndPermissions(string accessToken);
    }
}