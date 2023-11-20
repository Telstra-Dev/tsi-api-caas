/*using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Telstra.Common;

namespace Telstra.Core.Api
{
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileUploadMime = "multipart/form-data";
            if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
                return;

            var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));
            operation.RequestBody.Content[fileUploadMime].Schema.Properties =
                fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary"
                });
        }
    }

    public static class SwaggerServicesExtentions
    {
        public static void ConfigureSwaggerBehindProxy(this SwaggerOptions options)
        {
            options.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
            {
                if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;

                //var serverUrl = "$"{httpRequest.Headers["X-Forwarded-Proto"]}"://" +
                var serverUrl = "https://" +
                    $"{httpRequest.Headers["X-Forwarded-Host"]}/" +
                    $"{httpRequest.Headers["X-Forwarded-Prefix"]}";

                swaggerDoc.Servers = new List<OpenApiServer>()
                {
                    new OpenApiServer { Url = serverUrl }
                };
            });
        }
        public static void ConfigureSwaggerOptions(this SwaggerUIOptions options, AppSettings appSettings)
        {
            options.RoutePrefix = "swagger";
            options.SwaggerEndpoint("1.0/swagger.yaml", "WCA.Consumer.Api v1.0");
            options.OAuthClientId(appSettings.AzureAd.ClientId);
            options.OAuthUsePkce();
        }

        public static void AddSwagger(this IServiceCollection services, AppSettings appSettings)
        {
            services.AddSwaggerGen(c =>
            {
                var securityRequirementId = "AadBearer";
                var securityScheme = new OpenApiSecurityScheme
                {
                    OpenIdConnectUrl = new System.Uri(appSettings.AzureAd.OpenIdConnectConfigurationUrl),
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    Name = "Bearer",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new System.Uri(appSettings.AzureAd.AuthorizationUrl),
                            TokenUrl = new System.Uri(appSettings.AzureAd.TokenUrl),
                            Scopes = new Dictionary<string, string>
                               {
                                   { "openid", "Sign In Permissions" },
                                   { "profile", "User Profile Permissions" },
                                   { appSettings.AzureAd.DefaultScope, "" }
                               }
                        }
                    }
                };

                c.SwaggerDoc("1.0", new OpenApiInfo { Title = "WCA.Consumer.Api", Version = "1.0" });
                c.OperationFilter<SwaggerFileOperationFilter>();
                c.AddSecurityDefinition(securityRequirementId, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = securityRequirementId }
                        },
                        new [] { "openid", "profile", appSettings.AzureAd.DefaultScope }
                    }
                });
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }
    }
}*/
