using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;
using Telstra.Core.Data.Entities.StorageReponse;
using System.Net;
using System.Net.Http;
using Telstra.Common;
using WCA.Storage.Api.Proto;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WCA.Consumer.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient _grpcClient;
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public OrganisationService(WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient grpcClient, HttpClient httpClient, AppSettings appSettings,
                                    IMapper mapper, ILogger<OrganisationService> logger)
        {
            this._grpcClient = grpcClient;
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<IList<OrgSearchTreeNode>> GetOrganisationOverview()
        {
            _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
            var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/organisations/overview");
            var reply = await response.Content.ReadAsStringAsync();
            IList<Organisation> orgResponse = JsonConvert.DeserializeObject<IList<Organisation>>(reply);
            IList<OrgSearchTreeNode> orgList = _mapper.Map<IList<OrgSearchTreeNode>>(orgResponse);
            return orgList;
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
                CustomerName = "Test Account-01",
                Parent = "telstra-root-org",
                Id = "manual-test-customer-id",
                Children = children
            };
            return organisation;
        }

         /*public async Task<IList<OrgSearchTreeNode>> GetOrganisationOverview()
         {
            IList<OrgSearchTreeNode> orgSearchTreeNodes = new List<OrgSearchTreeNode>();
            Status status = new Status {
                Code = "RED",
                Reason = "Device status not found",
                Index = 3,
                Timestamp = 1657763038553
            };
            AddSearchTreeNode(orgSearchTreeNodes, "manual-test-customer-id", "Test Account-01",
                                "organisation", "/organisation/customer=manual-test-customer-id", status: status);

            AddSearchTreeNode(orgSearchTreeNodes, "5722000a-9552-4972-add4-32ca5f9a0c3b", "Child Org 1",
                                "organisation", "/organisations?customer=5722000a-9552-4972-add4-32ca5f9a0c3b", 
                                "manual-test-customer-id", status: status);

            AddSearchTreeNode(orgSearchTreeNodes, "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe", "Child Org 2",
                                "organisation", "/organisations?customer=1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe", 
                                "manual-test-customer-id", status: status);
            AddSearchTreeNode(orgSearchTreeNodes, "939d3cd5-38e7-4fc6-bbb7-802d27278f1e", "Grandchild Org 1",
                                "organisation", "/organisations?customer=939d3cd5-38e7-4fc6-bbb7-802d27278f1e", 
                                "5722000a-9552-4972-add4-32ca5f9a0c3b");   
            AddSearchTreeNode(orgSearchTreeNodes, "535c2ac9-5d32-41bc-9d7f-0b00f635a223", "Child Org Site 1",
                                "site", "/sites?customerId=5722000a-9552-4972-add4-32ca5f9a0c3b&siteId=535c2ac9-5d32-41bc-9d7f-0b00f635a223", 
                                "5722000a-9552-4972-add4-32ca5f9a0c3b", status);    
            AddSearchTreeNode(orgSearchTreeNodes, "c3903960-9abd-44e1-a4a2-fdc7964ca4c4", "Child Org Site 1",
                                "site", "/sites?customerId=1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe&siteId=c3903960-9abd-44e1-a4a2-fdc7964ca4c4", 
                                "1a6972f5-5be3-4d55-ab1f-c9c3182a2bbe", status);    
            AddSearchTreeNode(orgSearchTreeNodes, "bceead95-5b9d-47bc-9d93-4740db6c1292", "Blue Mile Area Wollongong",
                                "site", "/sites?customerId=manual-test-customer-id&siteId=bceead95-5b9d-47bc-9d93-4740db6c1292", 
                                "manual-test-customer-id", status); 
            AddSearchTreeNode(orgSearchTreeNodes, "3917acd9-2185-48a0-a71a-905316e2aae2", "Blue Mile Northern Gateway",
                                "gateway", "/devices/3917acd9-2185-48a0-a71a-905316e2aae2", 
                                "bceead95-5b9d-47bc-9d93-4740db6c1292"); 
            AddSearchTreeNode(orgSearchTreeNodes, "0448659b-eb21-410b-809c-c3b4879c9b48", "Blue Mile Northern Entrance",
                                "camera", "/devices/0448659b-eb21-410b-809c-c3b4879c9b48", 
                                "3917acd9-2185-48a0-a71a-905316e2aae2"); 
            return orgSearchTreeNodes;
        } */

        public Organisation CreateOrganisation(Organisation org)
        {
            return org;   
        }
        public Organisation UpdateOrganisation(string id, Organisation org)
        {
            return org;
        }
        public Organisation DeleteOrganisation(string id)
        {
            return new Organisation {
                        CustomerId = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e",
                        CustomerName = "Grandchild Org 1",
                        Parent = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        Alias = "TS",
                        CreatedAt = 1649906502253,
                        Id = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e"
                    };
        }

        private void AddSearchTreeNode(IList<OrgSearchTreeNode> orgSearchTreeNodes, string id, string text, string type, string href, 
                                        string parentId = null, Status status = null)
        {
            OrgSearchTreeNode orgSearchTreeNode = new OrgSearchTreeNode {
                Id = id,
                Text = text,
                Type = type,
                Href = href,
                ParentId = parentId,
                Status = status
            };
            orgSearchTreeNodes.Add(orgSearchTreeNode);
        }
    }
}