using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ISiteService
    {
        public Task<IList<SiteModel>> GetSitesForCustomer(string? customerId);
        public SiteModel GetSite(string? siteId);
        public Task<SiteModel> CreateSite(SiteModel site);
        public SiteModel UpdateSite(string siteId, SiteModel site);
        public SiteModel DeleteSite(string siteId);
    }
}
