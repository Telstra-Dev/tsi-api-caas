using System.Collections;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IDeviceService
    {
        public ArrayList GetDevices(string customerId, string siteId);
        public DeviceBase GetDevice(string deviceId);
    }
}
