using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;

namespace WCA.Consumer.Api.Services.Contracts
{
    public interface IOrganisationService
    {
        Task<OrganisationModel> GetOrganisation(string authorisationEmail, string customerId, bool includeChildren);
        Task<TenantOverview> GetOrganisationOverview(string authorisationEmail, bool includeHealthStatus);
        Task<OrganisationModel> CreateOrganisation(string authorisationEmail, OrganisationModel org);
        OrganisationModel UpdateOrganisation(string authorisationEmail, string id, OrganisationModel org);
        OrganisationModel DeleteOrganisation(string authorisationEmail, string id);
    }
}
