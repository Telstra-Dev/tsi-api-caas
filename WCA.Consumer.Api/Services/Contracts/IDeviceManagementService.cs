using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IDeviceManagementService
    {
        public Task<RtspFeedModel> GetRtspFeed(string authorisationEmail, string edgeDeviceId, string leafDeviceId);
    }
}
