using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]s")]
    public class SerialNumberController : BaseController
    {
        readonly ISerialNumberService service;

        public SerialNumberController(ISerialNumberService service)
        {
            this.service = service;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SerialNumberModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(List<SerialNumberModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public IActionResult GetSerialNumbers([FromQuery] string value, [FromQuery] string filter, [FromQuery] bool inactiveOnly = false, [FromQuery] uint? maxResults = null)
        {
            try {
                // Value query param has priority
                if (value != null)
                {
                    var serialNumber = this.service.GetSerialNumberByValue(value).Result;
                    if (serialNumber == null)
                    {
                        return NotFound(new { message = "No serial numbers could be found with the given criteria" });
                    }
                    else
                    {
                        return Ok(serialNumber);
                    }
                }

                var serialNumbers = this.service.GetSerialNumbersByFilter(filter, inactiveOnly, maxResults).Result;
                if (serialNumbers == null || serialNumbers.Count == 0)
                {
                    return NotFound(new { message = "No serial numbers could be found with the given criteria" });
                }
                else
                {
                    return Ok(serialNumbers);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}
