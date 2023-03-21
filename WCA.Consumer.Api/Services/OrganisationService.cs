using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System;
using System.Linq;
using Telstra.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Helpers;
using System.Threading;

namespace WCA.Consumer.Api.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient _grpcClient;
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IHealthStatusService _healthStatusService;

        public OrganisationService(WCA.Storage.Api.Proto.OrgOverview.OrgOverviewClient grpcClient,
                                    IRestClient httpClient,
                                    AppSettings appSettings,
                                    IMapper mapper,
                                    ILogger<OrganisationService> logger,
                                    IHealthStatusService healthStatusService)
        {
            _grpcClient = grpcClient;
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
            _healthStatusService = healthStatusService;
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
                throw new NullReferenceException("Invalid claim from token.");

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
                //Get org detail
                var orgResponse = await _httpClient.GetAsync<IList<Organisation>>($"{_appSettings.StorageAppHttp.BaseUri}/organisations{orgRequestSuffix}", CancellationToken.None);
                orgList = _mapper.Map<IList<OrgSearchTreeNode>>(orgResponse);
                _logger.LogInformation($"Organisations added to orgList for user {emailFromToken}");

                //Get sites detail
                var siteResponse = await _httpClient.GetAsync<IList<Site>>($"{_appSettings.StorageAppHttp.BaseUri}/sites{siteRequestSuffix}", CancellationToken.None);
                if (includeHealthStatus)
                {
                    foreach (var site in siteResponse)
                    {
                        var node = _mapper.Map<OrgSearchTreeNode>(site);
                        node.Status = await _healthStatusService.GetSiteHealthStatus(site);
                        orgList.Add(node);
                    }
                    _logger.LogInformation($"Sites and health status added to orgList for user {emailFromToken}");
                }
                else
                {
                    orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(siteResponse));
                    _logger.LogInformation($"Sites added to orgList for user {emailFromToken}");
                }

                //Get devices detail
                var deviceResponse = await _httpClient.GetAsync<IList<Device>>($"{_appSettings.StorageAppHttp.BaseUri}/devices{deviceRequestSuffix}", CancellationToken.None);
                if (includeHealthStatus)
                {
                    foreach (var device in deviceResponse)
                    {
                        var node = _mapper.Map<OrgSearchTreeNode>(device);
                        node.Status = await _healthStatusService.GetDeviceHealthStatus(device);
                        orgList.Add(node);
                    }
                    _logger.LogInformation($"Devices and health status added to orgList for user {emailFromToken}");
                }
                else
                {
                    orgList.AddRange(_mapper.Map<IList<OrgSearchTreeNode>>(deviceResponse));
                    _logger.LogInformation($"Devices added to orgList for user {emailFromToken}");
                }

                return orgList;
            }
            catch (Exception e)
            {
                _logger.LogError($"Fail to Get Organisation Overview for user {emailFromToken}: " + e.Message);
                throw new Exception(e.Message); ;
            }
        }

        public async Task<OrganisationModel> GetOrganisation(string customerId, bool includeChildren)
        {
            OrganisationModel foundMappedOrg = null;
            try
            {
                var foundOrgs = await _httpClient.GetAsync<IList<Organisation>>($"{_appSettings.StorageAppHttp.BaseUri}/organisations?customerId={customerId}", CancellationToken.None);
                if (foundOrgs != null)
                    foundMappedOrg = _mapper.Map<OrganisationModel>(foundOrgs.FirstOrDefault());

                return foundMappedOrg;
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to call GetOrganisation: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<OrganisationModel> CreateOrganisation(OrganisationModel newOrg)
        {
            var savedOrg = new OrganisationModel();
            try
            {
                var payload = JsonConvert.SerializeObject(newOrg);
                savedOrg = await _httpClient.PostAsync<OrganisationModel>($"{_appSettings.StorageAppHttp.BaseUri}/organisations", payload, CancellationToken.None);

                return savedOrg;
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to CreateOrganisation: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        //TO DO: Fake function
        public OrganisationModel UpdateOrganisation(string id, OrganisationModel org)
        {
            return org;
        }

        //TO DO: Fake function
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