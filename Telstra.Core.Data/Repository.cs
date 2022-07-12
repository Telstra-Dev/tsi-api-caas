using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telstra.Common;

namespace Telstra.Core.Data
{
    public abstract class Repository<TDbContext> : ReadOnlyRepository<TDbContext>, IDisposable, IRepository<TDbContext> where TDbContext : DbContext
    {
        protected Repository(IConfiguration config, TDbContext context, ILogger logger)
            : base(config, context, logger)
        {
            this.Context = context;
        }


        protected virtual IDictionary<Type, Func<Type, IQueryable>> GetTypeToDbSetCache()
        {
            return new Dictionary<Type, Func<Type, IQueryable>>();
        }

        protected override DbSet<T> InternalGetDbSet<T>()
        {
            DbSet<T> result = null;

            // Try to use the DbSet cache and fall back to the default behavior
            if (this.TypeToDbSetCache.Any())
            {
                var dbSetFound = this.TypeToDbSetCache.TryGetValue(typeof(T), out var getDbSetFunc);
                if (dbSetFound)
                {
                    var dbSetResult = getDbSetFunc.Invoke(typeof(T));
                    result = dbSetResult as DbSet<T>;
                }
            }

            return result ??= base.InternalGetDbSet<T>();
        }

        protected IDictionary<Type, Func<Type, IQueryable>> TypeToDbSetCache => _typeToDbSetCache ??= GetTypeToDbSetCache();

        #region Add, AddAsync, AddRange, and AddRangeAsync

        public TEntity Add<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Add));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.Add(entity);
            this.Context.SaveChanges();
            return entity;
        }

        public async Task<TEntity> AddAsync<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(AddAsync));

            var dbSet = this.InternalGetDbSet<TEntity>();
            var result = await dbSet.AddAsync(entity);
            return entity;
        }

        public void AddRange<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(AddRange));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.AddRange(entities);
        }

        public void AddRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(AddRange));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.AddRange(entities);
        }

        public async Task AddRangeAsync<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(AddRangeAsync));

            var dbSet = this.InternalGetDbSet<TEntity>();
            await dbSet.AddRangeAsync(entities);
        }

        public async Task AddRangeAsync<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(AddRangeAsync));

            var dbSet = this.InternalGetDbSet<TEntity>();
            await dbSet.AddRangeAsync(entities);
        }

        #endregion

        #region Remove and RemoveRange

        public void Remove<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Remove));

            Guard.NotNull(entity, nameof(entity));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.Remove(entity);
        }

        public void Remove<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Remove), key);

            var dbSet = this.InternalGetDbSet<TEntity>();
            var entity = this.Get<TEntity, TId>(key);
            dbSet.Remove(entity);
        }

        public void RemoveRange<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(RemoveRange));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.RemoveRange(entities);
        }

        public void RemoveRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(RemoveRange));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.RemoveRange(entities);
        }

        #endregion

        #region Update and UpdateRange        

        public void Update<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Update));

            Guard.NotNull(entity, nameof(entity));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.Update(entity);
        }

        public void UpdateRange<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(UpdateRange));

            var dbSet = this.InternalGetDbSet<TEntity>();
            dbSet.UpdateRange(entities);
        }

        public void UpdateRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(UpdateRange));

            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(entities, nameof(entities));

            var dbSet = this.InternalGetDbSet<TEntity>();
            // ReSharper disable once PossibleMultipleEnumeration
            dbSet.UpdateRange(entities);
        }

        #endregion

        #region SaveChanges and SaveChangesAsync        

        public void SaveChanges()
        {
            this.Logger?.LogTrace("Executing {0}.{1}()", this.GetType(), nameof(SaveChanges));

            this.Context.SaveChanges();
        }

        public void SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.Logger?.LogTrace("Executing {0}.{1}(acceptAllChangesOnSuccess={2})", this.GetType(), nameof(SaveChanges), acceptAllChangesOnSuccess);

            this.Context.SaveChanges(acceptAllChangesOnSuccess);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Logger?.LogTrace("Executing {0}.{1}(CancellationToken cancellationToken)", this.GetType(), nameof(SaveChangesAsync));

            await this.Context.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Logger?.LogTrace("Executing {0}.{1}(acceptAllChangesOnSuccess={2}, CancellationToken cancellationToken)",
                this.GetType(), nameof(SaveChanges), acceptAllChangesOnSuccess);

            await this.Context.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion

        #region Detach and Attach

        protected virtual TEntity Detach<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Detach));

            Guard.NotNull(entity, nameof(entity));

            var entry = this.Context.Entry(entity);
            entry.State = EntityState.Detached;
            return entity;
        }

        public virtual TEntity DetachIfNot<TEntity>(TEntity entity) where TEntity : class
        {
            return this.Context.Entry(entity).State != EntityState.Detached ? Detach(entity) : entity;
        }

        public void DetachRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] IEnumerable<TEntity> range) where TEntity : class
        {
            foreach (var entity in range)
            {
                this.DetachIfNot(entity);
            }
        }

        protected virtual TEntity Attach<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Attach));

            Guard.NotNull(entity, nameof(entity));

            var entry = this.Context.Attach(entity);
            entry.State = EntityState.Modified;
            return entity;
        }

        public virtual TEntity AttachIfNot<TEntity>(TEntity entity) where TEntity : class
        {
            return this.Context.Entry(entity).State == EntityState.Detached ? Attach(entity) : entity;
        }

        public void AttachRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] IEnumerable<TEntity> range) where TEntity : class
        {
            foreach (var entity in range)
            {
                this.AttachIfNot(entity);
            }
        }

        #endregion

        #region Dispose Methods

        public void Dispose()
        {
            this.Logger?.LogTrace("Executing {0}.{1}()", this.GetType(), nameof(Dispose));

            this.Dispose(true);
        }

        private bool _disposed = false;
        private IDictionary<Type, Func<Type, IQueryable>> _typeToDbSetCache;

        protected virtual void Dispose(bool disposing)
        {
            this.Logger?.LogTrace("Executing {0}.{1}(disposing={2})", this.GetType(), nameof(Dispose), disposing);

            if (!this._disposed)
            {
                if (disposing)
                {
                    this.Context.Dispose();
                }
            }

            this._disposed = true;
        }

        #endregion
    }
}
