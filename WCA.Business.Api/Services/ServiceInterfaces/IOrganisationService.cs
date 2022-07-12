using WCA.Business.Api.Models;

namespace WCA.Business.Api.Services.ServicesInterfaces
{
    public interface IOrganisationService
    {
        public OrganisationViewModel[] GetOrganisation(int customerId);
    }
}
