using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Helpers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("tag-manager")]
    public class TagManagerController : BaseController
    {
        readonly ITagManagerService _tagManagerService;

        public TagManagerController(ITagManagerService tagManagerService)
        {
            _tagManagerService = tagManagerService;
        }

        [HttpGet()]
        [ProducesResponseType(typeof(IList<TagModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTags(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail
        )
        {
            try
            {
                return Ok(await _tagManagerService.GetTagsAsync(authorisationEmail));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost()]
        [ProducesResponseType(typeof(IList<TagModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateTags(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] List<CreateTagModel> tags
        )
        {
            try
            {
                return Ok(await _tagManagerService.CreateTagsAsync(authorisationEmail, tags));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut()]
        [ProducesResponseType(typeof(TagModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RenameTag(
            [FromHeader(Name = "X-CUsername")] string authorisationEmail,
            [FromBody] TagModel tag
        )
        {
            try
            {
                return Ok(await _tagManagerService.RenameTagAsync(authorisationEmail, tag));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
