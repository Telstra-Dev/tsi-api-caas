using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ITagManagerService
    {
        public Task<List<TagModel>> GetTagsAsync(string token);
        public Task<int> CreateTagsAsync(List<CreateTagModel> tags, string token);
        public Task<TagModel> RenameTagAsync(TagModel tag, string token);
    }
}
