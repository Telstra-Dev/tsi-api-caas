using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace WCA.Business.Api.Controllers
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
