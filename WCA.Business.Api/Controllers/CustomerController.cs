﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WCA.Business.Api.Services.ServicesInterfaces;

namespace WCA.Business.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CustomerController : BaseController
    {
        readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            this._service = service;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async System.Threading.Tasks.Task<IActionResult> GetCustomerById(int id)
        {
            return Ok(await _service.GetCustomerById(id));
        }
    }
}
