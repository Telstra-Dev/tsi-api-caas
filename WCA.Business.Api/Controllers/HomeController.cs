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
    public class HomeController : BaseController
    {
        readonly IHomeService service;

        public HomeController(IHomeService service)
        {
            this.service = service;
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new { result = "API up and Running" });
        }

        [HttpGet("user/{id}")]
        public IActionResult GetUserById(int id)
        {
            return Ok(this.service.GetUserById(id));
        }
    }
}
