using System;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telstra.Common;

namespace Telstra.Core.Data.Contexts
{
    public static class StorageRegistrations
    {
        public static void RegisterDbContext<T>(this IServiceCollection @this, DBSetting setting) where T : DbContext
        {
            var builderOptions = new DbContextOptionsBuilder<T>();
            builderOptions
                    .UseNpgsql(setting.ConnectionString, options =>
                    {
                        options.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention();

            @this.AddScoped<T>(f => (T)Activator.CreateInstance(typeof(T), builderOptions.Options, setting.Schema));
        }

        public static void RegisterMultiTenantDbContext<T>(this IServiceCollection @this, DBSetting setting, ITenantInfo tenantInfo, string CustomSchema = null) where T : MultiTenantDbContext
        {
            var builderOptions = new DbContextOptionsBuilder<T>();
            builderOptions
                    .UseNpgsql(setting.ConnectionString, options =>
                    {
                        options.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention();

            @this.AddScoped<T>(f => (T)Activator.CreateInstance(typeof(T), tenantInfo, builderOptions.Options, CustomSchema ?? setting.Schema));
        }

        public static void RegisterMultiTenantDbContext<T>(this IServiceCollection @this, Func<IServiceProvider, T> provider) where T : MultiTenantDbContext
        {
            @this.AddScoped<T>(provider);
        }

        public static void RegisterMultiTenantStore<T>(this IServiceCollection @this, DBSetting setting, string CustomSchema = null) where T : EFCoreStoreDbContext<MyTenentInfo>
        {
            var builderOptions = new DbContextOptionsBuilder<T>();
            
            builderOptions
                    .UseNpgsql(setting.ConnectionString, options =>
                    {
                        options.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention();

            @this.AddScoped<T>(f => (T)Activator.CreateInstance(typeof(T), builderOptions.Options, CustomSchema ?? setting.Schema));
        }

        public static void RegisterContextBuilderOptions<T>(this IServiceCollection @this, DBSetting setting) where T : MultiTenantDbContext
        {
            var builderOptions = new DbContextOptionsBuilder<T>();
            builderOptions
                    .UseNpgsql(setting.ConnectionString, options =>
                    {
                        options.CommandTimeout(60);
                    })
                    .UseSnakeCaseNamingConvention();

            @this.AddScoped<DbContextOptions<T>>(f => builderOptions.Options);
        }
    }
}
