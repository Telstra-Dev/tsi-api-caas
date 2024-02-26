using System;
using System.IO.Compression;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Newtonsoft.Json.Serialization;
using Telstra.Common;
using WCA.Consumer.Api.Services.Contracts;
using WCA.Consumer.Api.Services;
using System.Text.Json;
using WCA.Consumer.Api;
using WCA.Consumer.Api.Extensions;

namespace Telstra.Core.Api
{
    public class Startup
    {
        private readonly AppSettings appSettings;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.appSettings = configuration.Bind<AppSettings>();
        }

        private readonly IConfiguration Configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddFeatureManagement();
            services.AddSwagger(this.appSettings);
            services.AddCors();
            services.AddLogging();
            services.AddControllers()
           .AddJsonOptions(options =>
           {
               options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
           });
            

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = null;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressConsumesConstraintForFormFileParameters = true;
                    options.SuppressInferBindingSourcesForParameters = true;
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;
                });
            services.AddLoggingAndTelemetry(Configuration);
            services.AddSwaggerGen();
            //services.AddAuth(Configuration, this.appSettings);
            services.AddMemoryCache();

            services.AddGrpcClient<WCA.Storage.Api.Proto.Customer.CustomerClient>(o =>
            {
                o.Address = new Uri(appSettings.StorageAppGrpc.BaseUri);
            });

            // TODO: evaluate if needed in future (customer service deprecated / was experimental for gRPC?)
            // services.AddHttpClient<CustomerService>();

            services.AddGrpcClient<WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient>(o =>
            {
                o.Address = new Uri(appSettings.StorageAppGrpc.BaseUri);
            });

            services.AddHttpClient<OrganisationService>();

            services.AddHealthChecks();
        }

        public virtual void ConfigureContainer(IServiceCollection container)
        {
            container.RegisterDependencies(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            
            app.RegisterGlobalExceptionHandler(loggerFactory, env.IsProduction());

            app.UseCors(x =>
                x.AllowAnyOrigin()
                 .AllowAnyHeader()
                 .AllowAnyMethod()
            );
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", () => new {
                    message = "CAAS API working OK"
                });
                
                endpoints.MapHealthChecks("/api/health", new HealthCheckOptions()
                {
                    AllowCachingResponses = false
                }).WithMetadata(new AllowAnonymousAttribute());
                endpoints.MapControllers();
            });
        }
    }
}
