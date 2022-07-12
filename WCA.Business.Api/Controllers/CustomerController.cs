using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telstra.Core.Contracts;
using Telstra.Core.Api.Helpers;
using Grpc.Net.Client;
using WCA.Storage.Api.Proto;
using System.Net;
using System.Net.Http;

namespace Telstra.Core.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CustomerController : BaseController
    {
        readonly ICustomerService service;

        public CustomerController(ICustomerService service)
        {
            this.service = service;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> GetCustomerById(int id)
        {
            // Add for proxy through system
            HttpClient.DefaultProxy = new WebProxy();

            using var channel = GrpcChannel.ForAddress("http://localhost:7565");
            var client = new Customer.CustomerClient(channel);
            var reply = await client.GetCustomerById2Async(
                new CustomerModelRequest { CustomerId = id });
            return Ok(new { result = "Customer Name = " + reply.Name });
        }
    }
}
