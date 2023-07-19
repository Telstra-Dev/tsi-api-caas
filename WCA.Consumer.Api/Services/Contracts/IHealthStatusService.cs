using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IHealthStatusService
    {
        Task<HealthStatusModel> GetHealthStatusFromDeviceId(string deviceId);
        Task<HealthStatusModel> GetDeviceHealthStatus(Device device);

        Task<HealthStatusModel> GetHealthStatusFromSiteId(string siteId);
        Task<HealthStatusModel> GetSiteHealthStatus(Site site);
        TenantOverview ConvertTimeToHealthStatus(TenantOverview overview);
    }
}
