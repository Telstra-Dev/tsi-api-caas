using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telstra.Common;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Services;
using Telstra.Core.Data.Contexts;
using Microsoft.AspNetCore.Http;
using Telstra.Core.Data;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using Telstra.Core.Repo;
using Microsoft.Extensions.Logging;

namespace Telstra.Core.Api
{
    public static class IoC
    {
        public static void RegisterDependencies(this IServiceCollection collection, IConfiguration configuration)
        {
            // REGISTER ALL THE DEPENDANCIES HERE
            var appSettings = configuration.Bind<AppSettings>();
            collection.AddSingleton(f => appSettings);
            collection.AddHttpContextAccessor();

            //collection.RegisterMultiTenantStore<TenantStoreDbContext>(appSettings.Storage.MyDb, "tenant");
            //collection.RegisterContextBuilderOptions<MyMultiTenantContext>(appSettings.Storage.MyDb);

            //collection.RegisterDbContext<MyDBContext>(appSettings.Storage.MyDb);
            //collection.RegisterMultiTenantDbContext<MyMultiTenantContext>(m => new MyMultiTenantContext(
            //    m.GetService<IHttpContextAccessor>().HttpContext.GetMultiTenantContext<MyTenentInfo>()?.TenantInfo,
            //    m.GetService<DbContextOptions<MyMultiTenantContext>>(),
            //    appSettings.Storage.MyDb.Schema)
            //);
            //collection.AddMultiTenancy().WithStaticStrategy("TELSTRA");
            //collection.AddScoped<BookingRepository>();
            //collection.AddScoped<MyMultitenantRepository>(m => new MyMultitenantRepository(
            //    m.GetService<MyMultiTenantContext>(), m.GetService<ILogger<MyMultiTenantContext>>()));

            collection.AddSingleton<IRestClient, RestClient>();
            collection.AddAutoMapper(typeof(Startup));
            // TODO: evaluate if needed in future (customer service deprecated / was experimental for gRPC?)
            // collection.AddScoped<ICustomerService, CustomerService>();
            collection.AddScoped<IHomeService, HomeService>();
            collection.AddScoped<IOrganisationService, OrganisationService>();
            collection.AddScoped<ISiteService, SiteService>();
            collection.AddScoped<IDeviceService, DeviceService>();
            collection.AddScoped<ISerialNumberService, SerialNumberService>();
            collection.AddScoped<IHealthStatusService, HealthStatusService>();
            collection.AddScoped<ITagManagerService, TagManagerService>();
            collection.AddScoped<IDeviceManagementService, DeviceManagementService>();
        }
    }
}
