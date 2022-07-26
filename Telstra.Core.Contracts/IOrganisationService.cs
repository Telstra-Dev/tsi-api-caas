using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IOrganisationService
    {
        public Organisation GetOrganisation(int customerId, bool includeChildren);
        public Task<IList<OrgSearchTreeNode>> GetOrganisationOverview();
        public Organisation CreateOrganisation(Organisation org);
        public Organisation UpdateOrganisation(string id, Organisation org);
        public Organisation DeleteOrganisation(string id);
    }
}
