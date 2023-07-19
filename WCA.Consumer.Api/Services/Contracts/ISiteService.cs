using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ISiteService
    {
        public Task<IList<SiteModel>> GetSitesFromToken(string token);
        public Task<IList<SiteModel>> GetSitesForCustomer(string customerId);

        public Task<SiteModel> GetSite(string siteId, string customerId);
        public Task<SiteTelemetryProperty> GetSiteTelProperties(string token, string siteId);

        public Task<SiteModel> CreateSite(SiteModel newSite);

        public Task<SiteModel> UpdateSite(string siteId, SiteModel site);

        public Task<SiteModel> DeleteSite(string siteId);
    }
}
