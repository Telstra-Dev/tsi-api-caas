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
using System.Net.Http;
using Device = Telstra.Core.Data.Entities.Device;

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

        public async Task<TenantOverview> GetOrganisationOverview(string token, bool includeHealthStatus)
        {
            var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
            if (string.IsNullOrEmpty(emailFromToken))
                throw new NullReferenceException("Invalid claim from token.");

            var overview = new TenantOverview();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/organisations/overview?email={emailFromToken}&withHealthStatus={includeHealthStatus}");
                overview = await _httpClient.SendAsync<TenantOverview>(request, CancellationToken.None);
            }
            catch (Exception ex)
            {
                var errMsg = $"Fail to get tenant overview data from downstream service for user {emailFromToken}. With health status is {includeHealthStatus}.";
                _logger.LogError($"{errMsg} System message: {ex.Message}");
                throw new Exception(errMsg);
            }

            try
            {
                if (includeHealthStatus && overview != null && overview.Sites.Count > 0)
                {
                    overview = await _healthStatusService.GetTenantHealthStatus(overview);
                }

                return overview;
            }
            catch (Exception ex)
            {
                var errMsg = $"Fail to generate tenant overview with health status for user {emailFromToken}";
                _logger.LogError($"{errMsg} System message: {ex.Message}");
                throw new Exception(errMsg);
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