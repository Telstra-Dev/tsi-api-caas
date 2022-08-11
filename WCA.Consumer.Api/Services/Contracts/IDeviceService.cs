using System.Collections;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IDeviceService
    {
        public Task<ArrayList> GetDevices(string customerId, string siteId);

        public DeviceModel GetDevice(string deviceId);

        public Task<DeviceModel> DeleteDevice(string customerId, string deviceId);

        public Camera UpdateCameraDevice(string id, Camera device);

        public Gateway UpdateEdgeDevice(string id, Gateway device);

        public Task<Camera> CreateCameraDevice(Camera device);
        
        public Task<Gateway> CreateEdgeDevice(Gateway device);
    }
}
