using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Telstra.Core.Data
{
    public interface IReadOnlyRepository
    {
        IQueryable<T> GetDbSet<T>(bool trackingEnabled = false, bool loadRelatedData = false) where T : class;

        TEntity Get<TEntity, TId>(TId key) where TEntity : class;
        Task<TEntity> GetAsync<TEntity, TId>(TId key) where TEntity : class;

        (bool, TEntity) TryGet<TEntity, TId>(TId id) where TEntity : class;
        Task<(bool, TEntity)> TryGetAsync<TEntity, TId>(TId id) where TEntity : class;

        TEntity Find<TEntity, TId>(TId key) where TEntity : class;
        IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;
        Task<TEntity> FindAsync<TEntity, TId>(TId key) where TEntity : class;
        Task<IQueryable<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class;

        bool KeyExists<TEntity, TId>(TId key) where TEntity : class;
        Task<bool> KeyExistsAsync<TEntity, TId>(TId key) where TEntity : class;
    }
}
