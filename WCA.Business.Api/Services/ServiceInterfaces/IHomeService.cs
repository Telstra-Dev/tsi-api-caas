using System.Threading.Tasks;
using Telstra.Core.Data.Entities;

namespace WCA.Business.Api.Services.ServicesInterfaces
{
    public interface IHomeService
    {
        Task<User> GetUserById(int UserId);
    }
}
