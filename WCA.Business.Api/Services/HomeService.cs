using System.Threading.Tasks;
using Telstra.Core.Repo;
using Telstra.Core.Data.Entities;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace WCA.Business.Api.Services
{
    public class HomeService : IHomeService
    {
        private MyMultitenantRepository _repo;
        public HomeService(MyMultitenantRepository Repo)
        {
            this._repo = Repo;
        }


        public async Task<User> GetUserById(int UserId)
        {
            return await this._repo.GetUser(UserId);
        }
    }
}
