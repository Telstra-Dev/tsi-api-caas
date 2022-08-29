using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System;
using System.Text;
using System.Net.Http;
using Telstra.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient _grpcClient;
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public OrganisationService(WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient grpcClient,
                                    HttpClient httpClient,
                                    AppSettings appSettings,
                                    IMapper mapper,
                                    ILogger<OrganisationService> logger)
        {
            this._grpcClient = grpcClient;
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<IList<OrgSearchTreeNode>> GetOrganisationOverview()
        {
            IList<OrgSearchTreeNode> orgList = new List<OrgSearchTreeNode>();
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/organisations/overview");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Organisation> orgResponse = JsonConvert.DeserializeObject<IList<Organisation>>(reply);
                    orgList = _mapper.Map<IList<OrgSearchTreeNode>>(orgResponse);
                }
                else
                {
                    _logger.LogError("GetOrganisationOverview failed with error: " + reply);
                    throw new Exception($"Error getting org overview. {response.StatusCode} Response code from downstream: " + response.StatusCode); 
                }

                response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites");
                reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Site> siteResponse = JsonConvert.DeserializeObject<IList<Site>>(reply);
                    orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(siteResponse));
                }
                else
                {
                    _logger.LogError("GetSites failed with error: " + reply);
                    throw new Exception("$Error getting sites for overview. {response.StatusCode} Response code from downstream: " + response.StatusCode); 
                }

                response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices");
                reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Device> deviceResponse = JsonConvert.DeserializeObject<IList<Device>>(reply);
                    orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(deviceResponse));
                }
                else
                {
                    _logger.LogError("GetDevices failed with error: " + reply);
                    throw new Exception($"Error getting devices for overview. {response.StatusCode} Response code from downstream: " + response.StatusCode); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetOrganisationOverview: " + e.Message);
                throw new Exception(e.Message);;
            }
            return orgList;
        }

        public OrganisationModel GetOrganisation(int customerId, bool includeChildren)
        {
            OrganisationModel[] grandChild = new OrganisationModel[1];
            OrganisationModel[] children = new OrganisationModel[2];
            if (includeChildren)
            {
                grandChild[0] = new OrganisationModel {
                        CustomerId = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e",
                        CustomerName = "Grandchild Org 1",
                        Parent = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        Alias = "TS",
                        CreatedAt = 1649906502253,
                        Id = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e"
                    };

                children[0] = new OrganisationModel {
                        CustomerId = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        CustomerName = "Child Org 1",
                        Parent = "manual-test-customer-id",
                        Alias = "TS",
                        CreatedAt = 1649906487737,
                        Id = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                        Children = grandChild
                    };
                children[1] = new OrganisationModel {
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
            
            OrganisationModel organisation = new OrganisationModel
            {
                CustomerId = "moreton-bay-customer-id",
                CustomerName = "Moreton Bay Regional Council",
                Parent = "telstra-root-org",
                Id = "moreton-bay-customer-id",
                Children = children
            };
            return organisation;
        }

        public async Task<OrganisationModel> CreateOrganisation(OrganisationModel newOrg)
        {
            OrganisationModel savedOrg = new OrganisationModel();
            try
            {
                var payload =JsonConvert.SerializeObject(newOrg);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/organisations", httpContent);
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    savedOrg = JsonConvert.DeserializeObject<OrganisationModel>(reply);
                }
                else
                {
                    _logger.LogError("CreateOrganisation failed with error: " + reply);
                    throw new Exception($"Error creating an organisation. {response.StatusCode} Response code from downstream: " + response.StatusCode); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CreateOrganisation: " + e.Message);
                throw new Exception(e.Message);
            }
            return savedOrg;
        }
        public OrganisationModel UpdateOrganisation(string id, OrganisationModel org)
        {
            return org;
        }
        public OrganisationModel DeleteOrganisation(string id)
        {
            return new OrganisationModel {
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