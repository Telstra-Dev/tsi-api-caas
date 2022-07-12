using System;
using Finbuckle.MultiTenant;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telstra.Common;
using Telstra.Core.Data.Contexts;

namespace Telstra.Core.Data
{
    public static class ServiceCollectionExtensions
    {
        public static FinbuckleMultiTenantBuilder<MyTenentInfo> AddMultiTenancy([NotNull] this IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services));

            var multiTenantBuilder = services.AddMultiTenant<MyTenentInfo>()
                .WithEFCoreStore<TenantStoreDbContext, MyTenentInfo>();

            return multiTenantBuilder;
        }
    }
}
