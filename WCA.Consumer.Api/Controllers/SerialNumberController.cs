using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading.Tasks;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]s")]
    public class SerialNumberController : BaseController
    {
        readonly ISerialNumberService _snService;

        public SerialNumberController(ISerialNumberService snService)
        {
            _snService = snService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(SerialNumberModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(List<SerialNumberModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSerialNumbers(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromQuery] string value,
            [FromQuery] string filter,
            [FromQuery] bool inactiveOnly = false,
            [FromQuery] uint? maxResults = null
        )
        {
            try
            {
                // Value query param has priority
                if (value != null)
                {
                    var serialNumber = await _snService.GetSerialNumberByValue(authorisationEmail, value);
                    if (serialNumber == null)
                    {
                        return NotFound(new { message = "No serial numbers could be found with the given criteria" });
                    }
                    else
                    {
                        return Ok(serialNumber);
                    }
                }

                var serialNumbers = await _snService.GetSerialNumbersByFilter(authorisationEmail, filter, inactiveOnly, maxResults);
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
