using System.Collections.Generic;
using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IOrganisationService
    {
        public Organisation GetOrganisation(int customerId, bool includeChildren);
        public IList<OrgSearchTreeNode> GetOrganisationSearchTree();
    }
}
