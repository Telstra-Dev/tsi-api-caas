using System;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Data.Contexts
{
    public class MyMultiTenantContext : MultiTenantDbContext
    {
        public DbSet<User> Users { get; set; }

        public string ContextSchema;
        public DbContextOptions ContextOptions;
        public MyMultiTenantContext(ITenantInfo info, DbContextOptions options, string ContextSchema = "dbo")
            : base(info, options)
        {
            this.ContextSchema = ContextSchema;
            this.ContextOptions = options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(ContextSchema);
            base.OnModelCreating(modelBuilder);
        }
    }
}
