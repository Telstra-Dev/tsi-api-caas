using System;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace Telstra.Core.Data.Contexts
{
    public class TenantStoreDbContext : EFCoreStoreDbContext<MyTenentInfo>
    {
        public string ContextSchema;

        public TenantStoreDbContext(DbContextOptions<TenantStoreDbContext> options, string ContextSchema = "tenants")
            : base(options)
        {
            this.ContextSchema = ContextSchema;
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
