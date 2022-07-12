using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Telstra.Core.Api
{
    public class Program
    {

        public static string ENV_PREFIX =>
            Environment.GetEnvironmentVariable("MYAPP_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            // Development, Production, MyEnvironment
        public static IConfiguration ConfigurationBuilder => new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile($"appsettings.{ENV_PREFIX}.json", true)
                    .AddEnvironmentVariables("MYAPP_")
                    .AddUserSecrets(typeof(Startup).GetTypeInfo().Assembly)
                    .Build();

        public static void Main(string[] args)
        {
            Console.WriteLine($"Environment - {ENV_PREFIX}");
            CreateHostBuilder(args, ConfigurationBuilder).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup(f => new Startup(configuration));
                    webBuilder.UseConfiguration(configuration);

                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        if ((configuration.GetValue<int?>("ports:http") ?? 0) != 0)
                            serverOptions.ListenAnyIP(configuration.GetValue<int>("ports:http"));

                        if ((configuration.GetValue<int?>("ports:https") ?? 0) != 0)
                            serverOptions.ListenAnyIP(configuration.GetValue<int>("ports:https"), options =>
                            {
                                if (configuration["certificate:path"] != "" && configuration["certificate:path"] != "NA")
                                    options.UseHttps(configuration["certificate:path"], configuration["certificate:password"]);
                                else
                                    options.UseHttps();
                            });
                    });
                });
        
    }
}
