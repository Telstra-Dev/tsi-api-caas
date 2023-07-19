using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IOrganisationService
    {
        Task<OrganisationModel> GetOrganisation(string customerId, bool includeChildren);
        Task<TenantOverview> GetLandingPageOverview(string token, bool includeHealthStatus);
        Task<IList<OrgSearchTreeNode>> GetOrganisationOverview(string token, bool includeHealthStatus);
        Task<OrganisationModel> CreateOrganisation(OrganisationModel org);
        OrganisationModel UpdateOrganisation(string id, OrganisationModel org);
        OrganisationModel DeleteOrganisation(string id);
    }
}
