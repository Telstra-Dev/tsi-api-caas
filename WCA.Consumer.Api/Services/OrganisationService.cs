using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System;
using System.Linq;
using System.Text;
using System.Net.Http;
using Telstra.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Helpers;

namespace WCA.Consumer.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient _grpcClient;
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IHealthStatusService _healthStatusService;

        public OrganisationService(WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient grpcClient,
                                    HttpClient httpClient,
                                    AppSettings appSettings,
                                    IMapper mapper,
                                    ILogger<OrganisationService> logger,
                                    IHealthStatusService healthStatusService)
        {
            this._grpcClient = grpcClient;
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
            this._healthStatusService = healthStatusService;
        }

        public async Task<IList<OrgSearchTreeNode>> GetOrganisationOverview(string token, bool includeHealthStatus = false)
        {
            // Authorisation
            //  - Currently, very basic pre-MVP tactical to allow WCC to have acces
            //  - Strategic is to move to CAIMAN + authorisation uplift in the database (TBD).
            var orgRequestSuffix = "";
            var siteRequestSuffix = "";
            var deviceRequestSuffix = "";

            var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
            if (string.IsNullOrEmpty(emailFromToken))
                throw new NullReferenceException("Unrecognized email domain from JWT token.");

            if (emailFromToken.Contains("wcc-") && emailFromToken.Contains("telstrasmartspacesdemo.onmicrosoft.com"))
            {
                //legacy logic for wcc login users
                orgRequestSuffix = $"?customerId=wcc-id";
                siteRequestSuffix = $"?customerId=wcc-id";
                deviceRequestSuffix = $"?customerId=wcc-id";
            }
            else
            {
                //use email claim identifier
                if (emailFromToken.Contains("@team.telstra.com"))
                {
                    //engineer access to all orgs, sites and devices
                    orgRequestSuffix = "/overview";
                }
                else
                {
                    orgRequestSuffix = $"?email={emailFromToken}";
                    siteRequestSuffix = $"?email={emailFromToken}";
                    deviceRequestSuffix = $"?email={emailFromToken}";
                }
            }

            IList<OrgSearchTreeNode> orgList = new List<OrgSearchTreeNode>();
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/organisations{orgRequestSuffix}");
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

                response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites{siteRequestSuffix}");
                reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Site> siteResponse = JsonConvert.DeserializeObject<IList<Site>>(reply);

                    if (includeHealthStatus)
                    {
                        foreach (var site in siteResponse)
                        {
                            var node = _mapper.Map<OrgSearchTreeNode>(site);
                            node.Status = await _healthStatusService.GetSiteHealthStatus(site);
                            orgList.Add(node);
                        }
                    }
                    else
                    {
                        orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(siteResponse));
                    }
                }
                else
                {
                    _logger.LogError("GetSites failed with error: " + reply);
                    throw new Exception("$Error getting sites for overview. {response.StatusCode} Response code from downstream: " + response.StatusCode);
                }

                response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices{deviceRequestSuffix}");
                reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Device> deviceResponse = JsonConvert.DeserializeObject<IList<Device>>(reply);

                    if (includeHealthStatus)
                    {
                        foreach (var device in deviceResponse)
                        {
                            var node = _mapper.Map<OrgSearchTreeNode>(device);
                            node.Status = await _healthStatusService.GetDeviceHealthStatus(device);
                            orgList.Add(node);
                        }
                    }
                    else
                    {
                        orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(deviceResponse));
                    }
                }
                else
                {
                    _logger.LogError("GetDevices failed with error: " + reply);
                    throw new Exception($"Error getting devices for overview. {response.StatusCode} Response code from downstream: " + response.StatusCode);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to Get Organisation Overview: " + e.Message);
                throw new Exception(e.Message); ;
            }

            return orgList;
        }

        public async Task<OrganisationModel> GetOrganisation(string customerId, bool includeChildren)
        {
            OrganisationModel foundMappedOrg = null;
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/organisations?customerId={customerId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Organisation> foundOrgs = JsonConvert.DeserializeObject<IList<Organisation>>(reply);
                    if (foundOrgs != null && foundOrgs.Count > 0)
                        foundMappedOrg = _mapper.Map<OrganisationModel>(foundOrgs.FirstOrDefault());
                }
                else
                {
                    _logger.LogError("GetOrganisation failed with error: " + reply);
                    throw new Exception($"Error getting an organisation. {response.StatusCode} Response code from downstream: " + response.StatusCode);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetOrganisation: " + e.Message);
                throw new Exception(e.Message);
            }
            return foundMappedOrg;
        }

        public async Task<OrganisationModel> CreateOrganisation(OrganisationModel newOrg)
        {
            OrganisationModel savedOrg = new OrganisationModel();
            try
            {
                var payload = JsonConvert.SerializeObject(newOrg);
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
            return new OrganisationModel
            {
                CustomerId = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e",
                CustomerName = "Grandchild Org 1",
                Parent = "5722000a-9552-4972-add4-32ca5f9a0c3b",
                Alias = "TS",
                CreatedAt = 1649906502253,
                Id = "939d3cd5-38e7-4fc6-bbb7-802d27278f1e"
            };
        }

        private void AddSearchTreeNode(IList<OrgSearchTreeNode> orgSearchTreeNodes, string id, string text, string type, string href,
                                        string parentId = null, HealthStatusModel status = null)
        {
            OrgSearchTreeNode orgSearchTreeNode = new OrgSearchTreeNode
            {
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