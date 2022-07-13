using Telstra.Core.Repo;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        public OrganisationService()
        {
        }

        public Organisation GetOrganisation(int customerId, bool includeChildren)
        {
            Organisation[] grandChild = new Organisation[1];
            Organisation[] children = new Organisation[2];
            if (includeChildren)
            {
                grandChild[0] = new Organisation {
                        CustomerId = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e",
                        CustomerName = "Grandchild Org 1",
                        Parent = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        Alias = "TS",
                        CreatedAt = 1649906502253,
                        Id = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e"
                    };

                children[0] = new Organisation {
                        CustomerId = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        CustomerName = "Child Org 1",
                        Parent = "manual-test-customer-id",
                        Alias = "TS",
                        CreatedAt = 1649906487737,
                        Id = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        Children = grandChild
                    };
                children[1] = new Organisation {
                        CustomerId = "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe",
                        CustomerName = "Child Org 2",
                        Parent = "manual-test-customer-id",
                        Alias = "TS",
                        CreatedAt = 1649907827892,
                        Id = "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe"
                    };
            }
            else
            {
                children=null;
            }
            
            Organisation organisation = new Organisation
            {
                CustomerId = "manual-test-customer-id",
                CustomerName = "Postman & Test Account-01",
                Parent = "telstra-root-org",
                Id = "manual-test-customer-id",
                Children = children
            };
            return organisation;
        }
    }
}