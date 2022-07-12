using System;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IHomeService
    {
        Task<User> GetUserById(int UserId);
    }
}
