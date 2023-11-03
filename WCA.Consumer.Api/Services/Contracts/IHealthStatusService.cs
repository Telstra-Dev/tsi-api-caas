using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IHealthStatusService
    {
        Task<HealthStatusModel> GetHealthStatusFromDeviceId(string authorisationEmail, string deviceId);
        Task<HealthStatusModel> GetDeviceHealthStatus(string authorisationEmail, Device device);

        Task<HealthStatusModel> GetHealthStatusFromSiteId(string authorisationEmail, string siteId);
        Task<HealthStatusModel> GetSiteHealthStatus(string authorisationEmail, Site site);
        // TenantOverview GetTenantHealthStatus(string authorisationEmail, TenantOverview overview);
        Task<TenantOverview> GetTenantHealthStatus(string authorisationEmail, TenantOverview overview);
    }
}
