using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telstra.Common;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Services;

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

            // @this.RegisterMultiTenantStore<TenantStoreDbContext>(appSettings.Storage.MyDb, "tenant");
            // @this.RegisterContextBuilderOptions<MyMultiTenantContext>(appSettings.Storage.MyDb);

            // @this.RegisterDbContext<MyDBContext>(appSettings.Storage.MyDb);
            // @this.RegisterMultiTenantDbContext<MyMultiTenantContext>(m => new MyMultiTenantContext(
            //     m.GetService<IHttpContextAccessor>().HttpContext.GetMultiTenantContext<MyTenentInfo>()?.TenantInfo,
            //     m.GetService<DbContextOptions<MyMultiTenantContext>>(),
            //     appSettings.Storage.MyDb.Schema)
            // );

            // @this.AddMultiTenancy().WithStaticStrategy("TELSTRA");

            // @this.AddScoped<BookingRepository>();
            // @this.AddScoped<MyMultitenantRepository>(m => new MyMultitenantRepository(
            //     m.GetService<MyMultiTenantContext>(), m.GetService<ILogger<MyMultiTenantContext>>()));
            collection.AddSingleton<IRestClient, RestClient>();
            collection.AddScoped<ICustomerService, CustomerService>();
            collection.AddScoped<IHomeService, HomeService>();
            collection.AddScoped<IOrganisationService, OrganisationService>();
            collection.AddScoped<ISiteService, SiteService>();
            collection.AddScoped<IDeviceService, DeviceService>();
            collection.AddScoped<ISerialNumberService, SerialNumberService>();
            collection.AddScoped<IHealthStatusService, HealthStatusService>();
        }
    }
}
