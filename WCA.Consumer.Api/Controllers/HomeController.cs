using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok(new { result = "Caas API up and Running" });
        }
    }
}
