using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Security.Authentication;
using Telstra.Common;
using Telstra.Core.Api;
using WCA.Consumer.Api.Extensions;
using WCA.Consumer.Api.Services;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Services.UMS;

namespace WCA.Consumer.Api
{
    public static class IoC
    {
        public static void RegisterDependencies(this IServiceCollection collection, IConfiguration configuration)
        {
            // REGISTER ALL THE DEPENDANCIES HERE
            var appSettings = configuration.Bind<AppSettings>();

            FlurlHttp.Clients.WithDefaults(builder =>
            {
                builder.ConfigureInnerHandler(handler =>
                {
                    handler.SslProtocols = SslProtocols.Tls12;
#if DEBUG
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif

                    handler.UseProxy = configuration.GetValue<bool>("UseDownstreamProxy");
                    handler.Proxy = new WebProxy(configuration.GetValue<string>("DownstreamProxy:host"), true);

                    if (!handler.UseProxy) return;
                    if (configuration.GetValue<bool>("DownstreamProxy:auth"))
                        handler.Credentials = new NetworkCredential(configuration.GetValue<string>("DownstreamProxy:username"), configuration.GetValue<string>("DownstreamProxy:password"));
                });
            });

#if DEBUG
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);
#endif

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
            collection.AddSingleton<IUserManagementService, UserManagementService>();
            collection.AddScoped<IUserService, UserService>();
        }
    }
}
