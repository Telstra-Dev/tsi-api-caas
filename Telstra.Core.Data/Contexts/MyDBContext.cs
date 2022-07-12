using System;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace Telstra.Core.Data.Contexts
{
    public class MyDBContext : DbContext
    {
        string ContextSchema;
        public MyDBContext(DbContextOptions options, string ContextSchema = "dbo")
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
