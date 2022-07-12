using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telstra.Common;

namespace Telstra.Core.Data
{
    public abstract class ReadOnlyRepository<TDbContext> : RepositoryBase, IReadOnlyRepository where TDbContext : DbContext
    {
        public IConfiguration Configuration { get; protected set; }
        public IServiceProvider Services { get; protected set; }
        protected ILogger Logger { get; set; }

        private TDbContext _context;

        protected ReadOnlyRepository([NotNull] IConfiguration config, TDbContext context, ILogger logger)
            : this(config, logger)
        {
            this._context = context;
        }

        protected ReadOnlyRepository([NotNull] IConfiguration config, ILogger logger)
        {
            this.ConfigureServices(config, logger);
        }

        private void ConfigureServices([NotNull] IConfiguration config, ILogger logger)
        {
            this.Configuration = config;
            this.Logger = logger;

            this.Logger?.LogInformation("Logging for {0} Initialized", this.GetType());
        }

        /// <summary>
        /// Gets or sets DbContext.
        /// </summary>
        public TDbContext Context
        {
            get => _context ??= this.Services?.GetRequiredService<TDbContext>();
            set => _context = value.IsNotNull<TDbContext>(nameof(value));
        }

        protected abstract IQueryable<T> LoadRelated<T>(IQueryable<T> dbSet);

        public virtual IQueryable<T> GetDbSet<T>(bool trackingEnabled = false, bool loadRelatedData = false) where T : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(trackingEnabled={2}, loadRelatedData={3})",
                this.GetType(), nameof(GetDbSet), trackingEnabled, loadRelatedData);

            var dbSet = this.InternalGetDbSet<T>();

            var setWithOptions = trackingEnabled ? dbSet.AsTracking() : dbSet.AsNoTracking();
            var result = setWithOptions;

            if (loadRelatedData)
            {
                result = LoadRelated(setWithOptions);
            }

            return result;
        }

        protected virtual DbSet<T> InternalGetDbSet<T>() where T : class
        {
            // By default lookup the DbSet using the default behavior
            var dbSet = this.Context.Set<T>();
            return dbSet;
        }

        public virtual TEntity Get<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Get), key);

            var dbSet = this.InternalGetDbSet<TEntity>();
            var entity = dbSet?.Find(key);
            return entity;
        }

        public virtual async Task<TEntity> GetAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(Get), key);

            var dbSet = this.InternalGetDbSet<TEntity>();
            var entity = dbSet != null ? await dbSet.FindAsync(key) : null;
            return entity;
        }

        public virtual (bool, TEntity) TryGet<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(TryGet), key);

            var dbSet = this.InternalGetDbSet<TEntity>();
            var entity = dbSet?.Find(key);
            var entityFound = (entity != null);
            return (entityFound, entity);
        }

        public virtual async Task<(bool, TEntity)> TryGetAsync<TEntity, TId>(TId key) where TEntity : class
        {
            this.Logger?.LogTrace("Executing {0}.{1}(key={2})", this.GetType(), nameof(TryGetAsync), key);

            var dbSet = this.InternalGetDbSet<TEntity>();
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

            var dbSet = this.InternalGetDbSet<TEntity>();
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

            var dbSet = this.InternalGetDbSet<TEntity>();
            var results = dbSet.Where(filter);
            return await Task.FromResult(results);
        }

        public IQueryable<TEntity> FindAll<TEntity>(params object[] keyValues)
            where TEntity : class
        {
            this.Logger?.LogTrace($"Executing {this.GetType()}.{nameof(FindAll)}({nameof(keyValues)}={keyValues})");

            var results = this.Context.FindAll<TEntity>(keyValues);

            return results;
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
    }
}
