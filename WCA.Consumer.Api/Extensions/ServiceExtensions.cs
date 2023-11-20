using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Telstra.Common;
using Telstra.Common.Models;

namespace Telstra.Core.Api
{
    public static class ServiceExtensions
    {
        public static T Bind<T>(this IConfiguration @this, string section = null)
        {
            var instance = Activator.CreateInstance(typeof(T));
            if (section.IsNull())
                @this.Bind(instance);
            else
                @this.GetSection(section).Bind(instance);
            return (T)instance;
        }

        public static void AddLoggingAndTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddLogging(builder =>
                builder.AddConsole()
                       .AddApplicationInsights()
            );
        }

        public static void AddAuth(this IServiceCollection services, IConfiguration configuration, AppSettings appSettings)
        {
            
            /*services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(configuration, appSettings.AzureAd.KeyInApplicationSettings)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddInMemoryTokenCaches();*/

            /*services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = appSettings.AzureAd.Authority;
                //options.Audience = appSettings.AzureAd.ClientId;
                options.Audience = "885e502b-8889-4dbc-9d2a-9b0a1c1ffb4f";
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async failedContext =>
                    {
                        // For debugging purposes only!
                        var s = $"AuthenticationFailed: {failedContext.Exception.Message}";
                        failedContext.Response.ContentLength = s.Length;
                        await failedContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(s), 0, s.Length);
                    }
                };
                options.Events.OnTokenValidated = async context =>
                {
                    await Task.Delay(0);
                };
            });*/

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                                            .RequireAuthenticatedUser()
                                            .Build();
            });

        }

    }
}
