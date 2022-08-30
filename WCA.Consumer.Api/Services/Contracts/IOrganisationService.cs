using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IOrganisationService
    {
        public Task<OrganisationModel> GetOrganisation(string customerId, bool includeChildren);
        public Task<IList<OrgSearchTreeNode>> GetOrganisationOverview();
        // public Task<IList<OrgSearchTreeNode>> GetOrganisationOverviewTest();
        public Task<OrganisationModel> CreateOrganisation(OrganisationModel org);
        public OrganisationModel UpdateOrganisation(string id, OrganisationModel org);
        public OrganisationModel DeleteOrganisation(string id);
    }
}
