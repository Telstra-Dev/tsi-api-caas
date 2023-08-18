using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IActionResult> GetTags()
        {
            try
            {
                return Ok(await _tagManagerService.GetTagsAsync(GetToken()));
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
        public async Task<IActionResult> CreateTags([FromBody] List<CreateTagModel> tags)
        {
            try
            {
                return Ok(await _tagManagerService.CreateTagsAsync(tags, GetToken()));
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
        public async Task<IActionResult> RenameTag([FromBody] TagModel tag)
        {
            try
            {
                return Ok(await _tagManagerService.RenameTagAsync(tag, GetToken()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetToken()
        {
            return HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        }
    }
}
