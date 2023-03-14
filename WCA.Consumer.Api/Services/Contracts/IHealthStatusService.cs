using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IHealthStatusService
    {
        Task<HealthStatusModel> GetDeviceHealthStatus(string deviceId);
        Task<HealthStatusModel> GetDeviceHealthStatus(Device device);

        Task<HealthStatusModel> GetSiteHealthStatus(string siteId);
        Task<HealthStatusModel> GetSiteHealthStatus(Site site);
    }
}
