using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IOrganisationService
    {
        public Organisation GetOrganisation(int customerId, bool includeChildren);
        public Task<IList<OrgSearchTreeNode>> GetOrganisationOverview();
        public Task<IList<OrgSearchTreeNode>> GetOrganisationOverviewTest();
        public Task<Organisation> CreateOrganisation(Organisation org);
        public Organisation UpdateOrganisation(string id, Organisation org);
        public Organisation DeleteOrganisation(string id);
    }
}
