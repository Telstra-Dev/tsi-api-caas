using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telstra.Common;
using Telstra.Core.Api;
using WCA.Consumer.Api.Extensions;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api
{
    public static class IoC
    {
        public static void RegisterDependencies(this IServiceCollection collection, IConfiguration configuration)
        {
            // REGISTER ALL THE DEPENDANCIES HERE
            var appSettings = configuration.Bind<AppSettings>();

            collection.AddApplicationInsightsTelemetry();
            
            collection.AddSingleton(f => appSettings);
            collection.AddHttpContextAccessor();
            
            collection.AddSingleton<IRestClient, RestClient>();
            collection.AddAutoMapper(typeof(Startup));
            
            
            
            
            // TODO: evaluate if needed in future (customer service deprecated / was experimental for gRPC?)
            // collection.AddScoped<ICustomerService, CustomerService>();
            collection.AddScoped<IHomeService, HomeService>();
            collection.AddScoped<IOrganisationService, OrganisationService>();
            collection.AddScoped<ISiteService, SiteService>();
            collection.AddScoped<IDeviceService, DeviceService>();
            collection.AddScoped<IHealthStatusService, HealthStatusService>();
            collection.AddScoped<ITagManagerService, TagManagerService>();
        }
    }
}
