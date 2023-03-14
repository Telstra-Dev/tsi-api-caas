using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IDeviceService
    {
        public Task<ArrayList> GetDevices(string customerId, string siteId);

        public Task<DeviceModel> GetDevice(string deviceId, string customerId);

        public Task<DeviceModel> DeleteDevice(string customerId, string deviceId);

        public Task<Camera> CreateCameraDevice(Camera device);

        public Task<Camera> UpdateCameraDevice(string id, Camera device);

        public Task<Gateway> CreateEdgeDevice(Gateway device);

        public Task<Gateway> UpdateEdgeDevice(string id, Gateway device);

        public Task<IList<Device>> GetLeafDevicesForGateway(string deviceId);
    }
}
