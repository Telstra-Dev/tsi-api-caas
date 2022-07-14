using System.Collections.Generic;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IOrganisationService
    {
        public Organisation GetOrganisation(int customerId, bool includeChildren);
        public IList<OrgSearchTreeNode> GetOrganisationSearchTree();
        public Organisation CreateOrganisation(Organisation org);
        public Organisation UpdateOrganisation(string id, Organisation org);
        public Organisation DeleteOrganisation(string id);
    }
}
