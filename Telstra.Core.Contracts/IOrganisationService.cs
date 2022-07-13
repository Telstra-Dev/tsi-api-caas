using Telstra.Core.Data.Entities;

namespace Telstra.Core.Contracts
{
    public interface IOrganisationService
    {
        public OrganisationViewModel GetOrganisation(int customerId, bool includeChildren);
    }
}
