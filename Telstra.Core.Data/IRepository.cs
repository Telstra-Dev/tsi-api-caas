using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Telstra.Core.Data
{
    public interface IRepository<out TDbContext> : IRepository where TDbContext : DbContext
    {
        TDbContext Context { get; }
    }

    public interface IRepository : IReadOnlyRepository
    {
        TEntity Add<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class;

        void AddRange<TEntity>(params TEntity[] entities) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        Task AddRangeAsync<TEntity>(params TEntity[] entities) where TEntity : class;
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void RemoveRange<TEntity>(TEntity[] entities) where TEntity : class;
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        void Update<TEntity>(TEntity entity) where TEntity : class;
        void UpdateRange<TEntity>(params TEntity[] entities) where TEntity : class;
        void UpdateRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        void SaveChanges();
        void SaveChanges(bool acceptAllChangesOnSuccess);
        Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
