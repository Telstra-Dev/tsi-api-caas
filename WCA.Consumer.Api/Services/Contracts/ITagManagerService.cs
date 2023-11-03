using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ITagManagerService
    {
        public Task<List<TagModel>> GetTagsAsync(string authorisationEmail);
        public Task<int> CreateTagsAsync(string authorisationEmail, List<CreateTagModel> tags);
        public Task<TagModel> RenameTagAsync(string authorisationEmail, TagModel tag);
    }
}
