using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telstra.Core.Data;
using Telstra.Core.Data.Contexts;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Repo
{
    public class MyMultitenantRepository : MultiTenantRepository<MyMultiTenantContext>
    {
        public MyMultitenantRepository(MyMultiTenantContext context, ILogger logger) : base(context, logger) { }
        
        public async Task<User> GetUser(int UserId)
        {
            var result = await this.GetAsync<User, int>(UserId);
            return result;
        }
    }
}
