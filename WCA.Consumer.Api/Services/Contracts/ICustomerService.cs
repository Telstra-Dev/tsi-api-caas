using System;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomerById(int id);
        Task<Customer> GetCustomerById2(int id);
    }
}

