using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telstra.Core.Data;
using Telstra.Core.Data.Contexts;

namespace Telstra.Core.Repo
{
    public class BookingRepository : Repository<MyDBContext>
    {
        public BookingRepository(IConfiguration configuration, MyDBContext context, ILogger<BookingRepository> logger) : base(configuration, context, logger)
        {

        }
        protected override IQueryable<T> LoadRelated<T>(IQueryable<T> dbSet)
        {
            throw new NotImplementedException();
        }
    }
}
