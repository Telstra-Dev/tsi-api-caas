using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface ISiteService
    {
        public Task<IList<SiteModel>> GetSitesForCustomer(string customerId);

        public Task<SiteModel> GetSite(string siteId);

        public Task<SiteModel> SaveSite(SiteModel newSite, bool isUpdate = false);

        public Task<SiteModel> UpdateSite(string siteId, SiteModel site);
        
        public Task<SiteModel> DeleteSite(string siteId);
    }
}
