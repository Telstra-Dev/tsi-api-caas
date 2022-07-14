using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telstra.Common;
using Telstra.Core.Api.Helpers;

namespace Telstra.Core.Api
{
    public static class AppBuilderExtensions
    {
        public static void RegisterMiddlewares(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            //Inject Fake AdUser for Development
            if (env.IsDevelopment() && !configuration.Bind<AppSettings>().UseAd)
            {
                app.Use(next => async context =>
                    {
                        var fakeADUser = new FakeADUserInjector();
                        fakeADUser.Inject(context);
                        await next(context);
                    });
            }
        }

        public static void UseAuth(this IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            var settings = configuration.Bind<AppSettings>();
            if (settings.UseAd)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }


        public static void CreateAssetsPaths(this IApplicationBuilder app, IConfiguration configuration)
        {
            var settings = configuration.Bind<AppSettings>();
            var workingDir = AppDomain.CurrentDomain.BaseDirectory;
            var static_paths = settings.StaticPaths;
            if (static_paths != null && static_paths.Length > 0)
            {
                static_paths.ForEachItem((item, index) =>
                {
                    var path = Path.Combine(workingDir, item);
                    Directory.CreateDirectory(path);
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(path),
                        RequestPath = new PathString("/" + item)
                    });
                });
            }
        }

        public static void RegisterGlobalExceptionHandler(this IApplicationBuilder app, ILoggerFactory loggerFactory, bool isProd)
        {
            // Global exception handler
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlerFeature != null)
                    {
                        var logger = loggerFactory.CreateLogger("Global exception logger");
                        logger.LogError(
                            500,
                            exceptionHandlerFeature.Error,
                            exceptionHandlerFeature.Error.Message);
                    }

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var responseMessage = //isProd ? new
                    //{
                    //    error = "An unexpected error happened. Try again later",
                    //    errorMessage = "An unexpected error happened. Try again later"
                    //} : 
                    new
                    {
                        error = exceptionHandlerFeature.Error.ToString(),
                        errorMessage = exceptionHandlerFeature.Error.Message
                    };
                    await context.Response.WriteAsJsonAsync(responseMessage);
                });
            });
        }

        public static void AttachResponseInterceptor(this IApplicationBuilder app)
        {
            // Response Interceptor
            //app.Use(async (context, next) =>
            //{
            //    context.Response.Headers.Add("X-Environment", Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT")?.ToUpper());
            //    await next.Invoke();
            //});
        }
    }
}
