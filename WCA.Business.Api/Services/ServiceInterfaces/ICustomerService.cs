using System.Threading.Tasks;
using WCA.Business.Api.Models;

namespace WCA.Business.Api.Services.ServicesInterfaces
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomerById(int id);
    }
}
