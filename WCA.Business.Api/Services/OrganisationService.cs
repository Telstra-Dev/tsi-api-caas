using Telstra.Core.Repo;
using WCA.Business.Api.Services.ServicesInterfaces;
using WCA.Business.Api.Models;

namespace WCA.Business.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        private MyMultitenantRepository _repo;
        public OrganisationService(MyMultitenantRepository Repo)
        {
            this._repo = Repo;
        }

        public OrganisationViewModel[] GetOrganisation(int customerId)
        {
            OrganisationViewModel[] grandChild = new OrganisationViewModel[1];
            grandChild[0] = new OrganisationViewModel {
                    CustomerId = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e",
                    CustomerName = "Grandchild Org 1",
                    Parent = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                    Alias = "TS",
                    CreatedAt = 1649906502253,
                    Id = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e"
                };
            OrganisationViewModel[] children = new OrganisationViewModel[2];
            children[0] = new OrganisationViewModel {
                    CustomerId = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                    CustomerName = "Child Org 1",
                    Parent = "manual-test-customer-id",
                    Alias = "TS",
                    CreatedAt = 1649906487737,
                    Id = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                    Children = grandChild
                };
            children[1] = new OrganisationViewModel {
                    CustomerId = "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe",
                    CustomerName = "Child Org 2",
                    Parent = "manual-test-customer-id",
                    Alias = "TS",
                    CreatedAt = 1649907827892,
                    Id = "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe"
                };
            OrganisationViewModel organisation = new OrganisationViewModel
            {
                CustomerId = "manual-test-customer-id",
                CustomerName = "Postman & Test Account-01",
                Parent = "telstra-root-org",
                Id = "manual-test-customer-id",
                Children = children
            };
            return children;
        }
    }
}