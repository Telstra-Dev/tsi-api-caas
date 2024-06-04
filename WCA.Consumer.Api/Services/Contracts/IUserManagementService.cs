using System.Threading.Tasks;
using WCA.Consumer.Api.Models.UMS;
using static WCA.Consumer.Api.Models.UMS.UmsOperationModels;

namespace WCA.Consumer.Api.Services
{
    public interface IUserManagementService
    {
        Task<UmsUser> SearchByEmail(string email, string accessToken);
        Task<UmsUser> CreateUser(UmsAddUserRequest request, string accessToken);
        Task<UmsUser> UpdateUser(UmsUpdateUserRequest request, string userId, string accessToken);
        Task<UmsPermissionResponse> FetchRolePermissions(UmsPermissionRequest request, string accessToken);
        Task<UmsRolePermissionResponseModel> FetchRolesAndPermissions(string accessToken);
        Task<UmsUsersResponseModel> GetAllUsers(string accessToken);
    }
}