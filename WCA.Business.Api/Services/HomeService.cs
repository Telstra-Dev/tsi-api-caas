using System;
using System.Threading.Tasks;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;
using Telstra.Core.Repo;

namespace Telstra.Core.Api.Services
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
