using System.Collections.Generic;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface ISiteService
    {
        public IList<Site> GetSitesForCustomer(string? customerId);
        public Site GetSite(string? siteId);
        public Site CreateSite(Site site);
        public Site UpdateSite(string siteId, Site site);
        public Site DeleteSite(string siteId);
    }
}
