using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telstra.Common;
using Telstra.Core.Data;
using Telstra.Core.Data.Contexts;
using Telstra.Core.Repo;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant;
using WCA.Business.Api.Services;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace Telstra.Core.Api
{
    public static class IoC
    {
        public static void RegisterDependencies(this IServiceCollection @this, IConfiguration configuration)
        {
            // REGISTER ALL THE DEPENDANCIES HERE
            var appSettings = configuration.Bind<AppSettings>();
            @this.AddSingleton(f => appSettings);
            @this.AddHttpContextAccessor();

            
            @this.RegisterMultiTenantStore<TenantStoreDbContext>(appSettings.Storage.MyDb, "tenant");
            @this.RegisterContextBuilderOptions<MyMultiTenantContext>(appSettings.Storage.MyDb);

            @this.RegisterDbContext<MyDBContext>(appSettings.Storage.MyDb);
            @this.RegisterMultiTenantDbContext<MyMultiTenantContext>(m => new MyMultiTenantContext(
                m.GetService<IHttpContextAccessor>().HttpContext.GetMultiTenantContext<MyTenentInfo>()?.TenantInfo,
                m.GetService<DbContextOptions<MyMultiTenantContext>>(),
                appSettings.Storage.MyDb.Schema)
            );

            @this.AddMultiTenancy().WithStaticStrategy("TELSTRA");

            @this.AddScoped<BookingRepository>();
            @this.AddScoped<MyMultitenantRepository>(m => new MyMultitenantRepository(
                m.GetService<MyMultiTenantContext>(), m.GetService<ILogger<MyMultiTenantContext>>()));

            @this.AddScoped<ICustomerService, CustomerService>();
            @this.AddScoped<IHomeService, HomeService>();
            @this.AddScoped<IOrganisationService, OrganisationService>();
        }
    }
}
