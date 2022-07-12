using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telstra.Common;

namespace Telstra.Core.Data
{
    public abstract partial class MultiTenantRepository<TDbContext> : IDisposable where TDbContext : MultiTenantDbContext {
        private ConcurrentDictionary<string, TDbContext> ContextCache { get; set; } = new();
        public IServiceProvider Services { get; protected set; }
        protected ILogger Logger { get; set; }
        protected ITenantInfo tenantInfo;

        private DbContextOptions<TDbContext> _contextOptions;
        private TDbContext dbContext;

        protected virtual TDbContext InternalCreateDbContext()
        {
            if (this.dbContext != null) return this.dbContext;
            return null;
        }

        public DbContextOptions<TDbContext> ContextOptions
        {
            get => _contextOptions ??= this.Services?.GetRequiredService<DbContextOptions<TDbContext>>();
            set => _contextOptions = Guard.NotNull(value, nameof(value));
        }

        protected MultiTenantRepository(TDbContext context, ILogger logger)
        {
            this.dbContext = context;
            this.tenantInfo = context.TenantInfo;
            this.ConfigureServices(logger);
        }

        protected MultiTenantRepository(ITenantInfo tenantInfo, ILogger logger, DbContextOptions<TDbContext> contextOptions)
        {
            this._contextOptions = contextOptions;
            this.tenantInfo = tenantInfo;

            this.ConfigureServices(logger);
        }

        private void ConfigureServices(ILogger logger)
        {
            this.Logger = logger;
            this.Logger?.LogInformation("Logging for {0} Initialized", this.GetType());
        }

        protected DbSet<T> InternalGetDbSet<T>(ITenantInfo tenantInfo) where T : class
        {
            var context = this.GetDbContext(tenantInfo);
            var dbSet = context.Set<T>();
            return dbSet;
        }

        protected virtual IQueryable<T> GetDbSet<T>([NotNull] TDbContext context, bool trackingEnabled = false, bool loadRelatedData = false) where T : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(trackingEnabled={2}, loadRelatedData={3})",
                this.GetType(), nameof(GetDbSet), trackingEnabled, loadRelatedData);

            var dbSet = context.Set<T>();

            var setWithOptions = trackingEnabled ? dbSet.AsTracking() : dbSet.AsNoTracking();
            var result = setWithOptions;

            if (loadRelatedData)
            {
                result = LoadRelated(setWithOptions);
            }

            return result;
        }

        protected virtual IQueryable<T> LoadRelated<T>(IQueryable<T> dbSet)
        {
            return dbSet;
        }

        public TDbContext GetContext(ITenantInfo tenantInfo)
        {
            return this.GetDbContext(tenantInfo);
        }

        protected virtual TDbContext GetDbContext(ITenantInfo tenantInfo)
        {
            if (!this.ContextCache.TryGetValue(tenantInfo.Identifier, out var result))
            {
                result = this.dbContext ?? this.InternalCreateDbContext();
                this.ContextCache.TryAdd(tenantInfo.Identifier, result);
            }

            return result;
        }

        

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var context in this.ContextCache.Values)
                {
                    try
                    {
                        context.Dispose();
                    }
                    catch (Exception e)
                    {
                        this.Logger?.LogError($"Exception while disposing: {e}");
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual TEntity Get<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Get), key);

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var entity = dbSet?.Find(key);
            return entity;
        }

        public virtual async Task<TEntity> GetAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Get), key);

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var entity = dbSet != null ? await dbSet.FindAsync(key) : null;
            return entity;
        }

        public virtual (bool, TEntity) TryGet<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(TryGet), key);

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var entity = dbSet?.Find(key);
            var entityFound = (entity != null);
            return (entityFound, entity);
        }

        public virtual async Task<(bool, TEntity)> TryGetAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(TryGetAsync), key);

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var entity = dbSet != null ? await dbSet.FindAsync(key) : null;
            var entityFound = (entity != null);
            return (entityFound, entity);
        }

        public TEntity Find<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Find), key);

            return this.Get<TEntity, TId>(key);
        }

        public IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(filter)", this.GetType(), nameof(Find));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var results = dbSet.Where(filter);
            return results;
        }

        public async Task<TEntity> FindAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(FindAsync), key);

            return await this.GetAsync<TEntity, TId>(key);
        }

        public async Task<IQueryable<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            this.Logger?.LogTrace($"Executing {this.GetType()}.{nameof(FindAsync)}(filter)");

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var results = dbSet.Where(filter);
            return await Task.FromResult(results);
        }

        public bool KeyExists<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace($"Executing {this.GetType()}.{nameof(KeyExists)}(nameof(key)={key})", this.GetType(), nameof(KeyExists), key);

            var (keyFound, _) = TryGet<TEntity, TId>(key);
            return keyFound;
        }

        public async Task<bool> KeyExistsAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace($"Executing {this.GetType()}.{nameof(KeyExistsAsync)}(key={key})");

            var (keyFound, _) = await TryGetAsync<TEntity, TId>(key);
            return keyFound;
        }


        #region Add, AddAsync, AddRange, and AddRangeAsync

        public TEntity Add<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Add));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.Add(entity);
            this.GetDbContext(tenantInfo).SaveChanges();
            return entity;
        }

        public async Task<TEntity> AddAsync<TEntity>([NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(AddAsync));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var result = await dbSet.AddAsync(entity);
            return entity;
        }

        public void AddRange<TEntity>([NotNull] [ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(AddRange));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.AddRange(entities);
        }

        public void AddRange<TEntity>([ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(AddRange));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);

            dbSet.AddRange(entities);
        }

        public async Task AddRangeAsync<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(AddRangeAsync));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            await dbSet.AddRangeAsync(entities);
        }

        public async Task AddRangeAsync<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(AddRangeAsync));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            await dbSet.AddRangeAsync(entities);
        }

        #endregion

        #region Remove and RemoveRange

        public void Remove<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Remove));

            Guard.NotNull(entity, nameof(entity));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.Remove(entity);
        }

        public void Remove<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Remove), key);

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            var entity = this.Get<TEntity, TId>(key);
            dbSet.Remove(entity);
        }

        public void RemoveRange<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(RemoveRange));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.RemoveRange(entities);
        }

        public void RemoveRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(RemoveRange));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.RemoveRange(entities);
        }

        #endregion

        #region Update and UpdateRange        

        public void Update<TEntity>([System.Diagnostics.CodeAnalysis.NotNull] TEntity entity) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(TEntity entity)", this.GetType(), nameof(Update));

            Guard.NotNull(entity, nameof(entity));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.Update(entity);
        }

        public void UpdateRange<TEntity>([ItemNotNull] params TEntity[] entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(params TEntity[] entities)", this.GetType(), nameof(UpdateRange));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            dbSet.UpdateRange(entities);
        }

        public void UpdateRange<TEntity>([System.Diagnostics.CodeAnalysis.NotNull][ItemNotNull] IEnumerable<TEntity> entities) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(IEnumerable<TEntity> entities)", this.GetType(), nameof(UpdateRange));

            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(entities, nameof(entities));

            var dbSet = this.InternalGetDbSet<TEntity>(tenantInfo);
            // ReSharper disable once PossibleMultipleEnumeration
            dbSet.UpdateRange(entities);
        }

        #endregion

        #region SaveChanges and SaveChangesAsync        

        public void SaveChanges(ITenantInfo tenantInfo)
        {
            this.Logger?.LogTrace("Executing {0}.{1}()", this.GetType(), nameof(SaveChanges));

            this.GetDbContext(tenantInfo).SaveChanges();
        }

        public void SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.Logger?.LogTrace("Executing {0}.{1}(acceptAllChangesOnSuccess={2})", this.GetType(), nameof(SaveChanges), acceptAllChangesOnSuccess);

            this.GetDbContext(tenantInfo).SaveChanges(acceptAllChangesOnSuccess);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Logger?.LogTrace("Executing {0}.{1}(CancellationToken cancellationToken)", this.GetType(), nameof(SaveChangesAsync));

            await this.GetDbContext(tenantInfo).SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            this.Logger?.LogTrace("Executing {0}.{1}(acceptAllChangesOnSuccess={2}, CancellationToken cancellationToken)",
                this.GetType(), nameof(SaveChanges), acceptAllChangesOnSuccess);

            await this.GetDbContext(tenantInfo).SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        #endregion
    }
}
