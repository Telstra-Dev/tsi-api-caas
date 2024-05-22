using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ISiteService
    {
        public Task<IList<SiteModel>> GetSites(string authorisationEmail);

        Task<IList<SiteTagGroup>> GetSitesGroupedByTags(string authorisationEmail);

        // TODO (@Jason): Consider removing the following once integration points are confirmed, since it doesn't seem compatible with our auth strategy.
        public Task<IList<SiteModel>> GetSitesForCustomer(string authorisationEmail, string customerId);

        public Task<SiteModel> GetSite(string authorisationEmail, string siteId, string customerId);
        public Task<SiteModel> GetSiteById(string authorisationEmail, int siteId);
        public Task<SiteTelemetryProperty> GetSiteTelProperties(string authorisationEmail, int siteId);
        public Task<int> CreateOrUpdateSite(string authorisationEmail, SiteModel site);
        public Task<bool> DeleteSite(string authorisationEmail, string siteId);
        public Task<List<TelemetryLocation>> GetLocationTelemetry(string authorisationEmail, List<int> siteIds);
    }
}
