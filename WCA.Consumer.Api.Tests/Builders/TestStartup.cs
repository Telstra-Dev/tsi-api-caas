using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Telstra.Consumer.Api.Tests.Builders
{
    public class TestStartup : Startup
    {
        public TestStartup(
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
        : base(configuration)
        {
            
        }
    }
}
