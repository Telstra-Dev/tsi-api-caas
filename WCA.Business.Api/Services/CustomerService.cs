using System;
using System.Threading.Tasks;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;
using Telstra.Core.Repo;

namespace Telstra.Core.Api.Services
{
    public class CustomerService : ICustomerService
    {
        private MyMultitenantRepository _repo;
        public CustomerService(MyMultitenantRepository Repo)
        {
            this._repo = Repo;
        }
    }
}
