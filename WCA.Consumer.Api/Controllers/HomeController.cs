using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telstra.Core.Contracts;

namespace WCA.Consumer.Api.Controllers
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
    }
}
