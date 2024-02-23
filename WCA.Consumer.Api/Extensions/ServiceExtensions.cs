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
    }
}
