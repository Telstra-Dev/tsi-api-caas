using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telstra.Common;

namespace WCA.Consumer.Api.Extensions
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
