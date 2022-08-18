using System.Collections;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IDeviceService
    {
        public Task<ArrayList> GetDevices(string customerId, string siteId);

        public Task<DeviceModel> GetDevice(string deviceId);

        public Task<DeviceModel> DeleteDevice(string customerId, string deviceId);

        public Task<Camera> CreateCameraDevice(Camera device);

        public Task<Camera> UpdateCameraDevice(string id, Camera device);

        public Task<Gateway> CreateEdgeDevice(Gateway device);

        public Task<Gateway> UpdateEdgeDevice(string id, Gateway device);        
    }
}
