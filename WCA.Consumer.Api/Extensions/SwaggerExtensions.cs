using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Telstra.Core.Api
{
    public static class SwaggerServicesExtentions
    {
        public static void ConfigureSwaggerOptions(this SwaggerUIOptions options)
        {
            options.SwaggerEndpoint("https://ss2devv1fd.azurefd.net/wca_api/caas/swagger/v1/swagger.json", "WCA.Consumer.Api v1");
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WCA.Consumer.Api", Version = "v1" });
            });
        }
    }
}
