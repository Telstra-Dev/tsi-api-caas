using System.Collections;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IDeviceService
    {
        public ArrayList GetDevices(string customerId, string siteId);
        public DeviceBase GetDevice(string deviceId);
        public Camera UpdateCameraDevice(string id, Camera device);
        public Gateway UpdateEdgeDevice(string id, Gateway device);
        public Camera CreateCameraDevice(Camera device);
        public Gateway CreateEdgeDevice(Gateway device);
    }
}
