using Telstra.Core.Repo;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace WCA.Business.Api.Services
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
