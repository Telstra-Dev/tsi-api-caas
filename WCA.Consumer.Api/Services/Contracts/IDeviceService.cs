using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IDeviceService
    {
        public Task<ArrayList> GetDevices(string authorisationEmail, string customerId, string siteId);
        public Task<IList<Device>> GetGatewayDevices(string authorisationEmail, string customerId, string siteId);

        public Task<DeviceModel> GetDevice(string authorisationEmail, string deviceId, string customerId);

        public Task<DeviceModel> DeleteDevice(string authorisationEmail, string customerId, string deviceId);

        public Task<Camera> CreateCameraDevice(string authorisationEmail, Camera device);

        public Task<Camera> UpdateCameraDevice(string authorisationEmail, string id, Camera device);

        public Task<Gateway> CreateEdgeDevice(string authorisationEmail, Gateway device);

        public Task<Gateway> UpdateEdgeDevice(string authorisationEmail, string id, Gateway device);

        public Task<IList<Device>> GetLeafDevicesForGateway(string authorisationEmail, string deviceId);

        public Task<List<EdgeDeviceModel>> GetEdgeDevices(string authorisationEmail);
        public Task<List<LeafDeviceModel>> GetLeafDevices(string authorisationEmail);
        public Task<EdgeDeviceModel> GetEdgeDevice(string authorisationEmail, string deviceId);
        public Task<LeafDeviceModel> GetLeafDevice(string authorisationEmail, string deviceId);
        public Task<EdgeDeviceModel> UpdateTsiEdgeDevice(string authorisationEmail, EdgeDeviceModel edgeDevice);
        public Task<LeafDeviceModel> UpdateLeafDevice(string authorisationEmail, LeafDeviceModel leafDevice);
        public Task<EdgeDeviceModel> CreateEdgeDevice(string authorisationEmail, EdgeDeviceModel edgeDevice);
        public Task<LeafDeviceModel> CreateLeafDevice(string authorisationEmail, LeafDeviceModel edgeDevice);
        public Task<EdgeDeviceModel> DeleteEdgeDevice(string authorisationEmail, EdgeDeviceModel edgeDevice);
        public Task<LeafDeviceModel> DeleteLeafDevice(string authorisationEmail, LeafDeviceModel edgeDevice);
    }
}
