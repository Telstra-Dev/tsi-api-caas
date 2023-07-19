using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading;
using WCA.Consumer.Api.Helpers;
using System.Linq;

namespace WCA.Consumer.Api.Services
{
    public class SiteService : ISiteService
    {
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SiteService(IRestClient httpClient,
                        AppSettings appSettings,
                        IMapper mapper,
                        ILogger<SiteService> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IList<SiteModel>> GetSitesFromToken(string token)
        {
            var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
            if (string.IsNullOrEmpty(emailFromToken))
                throw new NullReferenceException("Invalid claim from token.");

            try
            {
                var foundSites = await _httpClient.GetAsync<IList<SiteNameModel>>($"{_appSettings.StorageAppHttp.BaseUri}/sites/names?email={emailFromToken}", CancellationToken.None);

                return foundSites.Select(x => new SiteModel { SiteId = x.Id, Name = x.DisplayName }).ToList();
            }
            catch (Exception e)
            {
                var errorMsg = "Fail to GetSitesForToken: " + e.Message;
                _logger.LogError(errorMsg);
                throw new Exception(errorMsg);
            }
        }

        public async Task<IList<SiteModel>> GetSitesForCustomer(string customerId)
        {
            IList<Site> sites = new List<Site>();
            IList<SiteModel> foundMappedSites = null;
            try
            {
                var foundSites = await _httpClient.GetAsync<IList<Site>>($"{_appSettings.StorageAppHttp.BaseUri}/sites?customerId={customerId}", CancellationToken.None);
                if (foundSites?.Count > 0)
                    foundMappedSites = _mapper.Map<IList<SiteModel>>(foundSites);

                return foundMappedSites;
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to GetSitesForCustomer: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<SiteTelemetryProperty> GetSiteTelProperties(string token, string siteId)
        {
            try
            {
                var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
                if (string.IsNullOrEmpty(emailFromToken))
                    throw new NullReferenceException("Invalid claim from token.");

                var downstreamResult = await _httpClient.GetAsync<SiteTelemetryProperty>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}/locations?email={emailFromToken}", CancellationToken.None);

                return downstreamResult;

            }
            catch (Exception e)
            {
                _logger.LogError("Get site location error: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<SiteModel> GetSite(string siteId, string customerId)
        {
            IList<Site> sites = new List<Site>();
            SiteModel foundMappedSite = null;
            try
            {
                var includeCustomerId = "";
                if (customerId != null)
                {
                    includeCustomerId = $"?customerId={customerId}";
                }
                var foundSite = await _httpClient.GetAsync<Site>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}{includeCustomerId}", CancellationToken.None);
                if (foundSite != null)
                    foundMappedSite = _mapper.Map<SiteModel>(foundSite);
                return foundMappedSite;
            }
            catch (Exception e)
            {
                _logger.LogError("GetSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<SiteModel> CreateSite(SiteModel newSite)
        {
            return await SaveSite(newSite);
        }

        public async Task<SiteModel> UpdateSite(string siteId, SiteModel updateSite)
        {
            return await SaveSite(updateSite, true);
        }

        public async Task<SiteModel> DeleteSite(string siteId)
        {
            SiteModel mappedDeletedSite = null;
            try
            {
                var deletedSite = await _httpClient.DeleteAsync<Site>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}", CancellationToken.None);
                mappedDeletedSite = _mapper.Map<SiteModel>(deletedSite);

                return mappedDeletedSite;
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to DeleteSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        private async Task<SiteModel> SaveSite(SiteModel newSite, bool isUpdate = false)
        {
            Site savedSite;
            try
            {
                Site mappedSite = _mapper.Map<Site>(newSite);
                var payload = JsonConvert.SerializeObject(mappedSite);
                if (!isUpdate)
                {
                    savedSite = await _httpClient.PostAsync<Site>($"{_appSettings.StorageAppHttp.BaseUri}/sites", payload, CancellationToken.None);
                }
                else
                {
                    savedSite = await _httpClient.PutAsync<Site>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{newSite.SiteId}", payload, CancellationToken.None);
                }

                return _mapper.Map<SiteModel>(savedSite);
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to SaveSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}