using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net.Http;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Services
{
    public class SiteService : ISiteService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SiteService(HttpClient httpClient,
                        AppSettings appSettings, 
                        IMapper mapper, 
                        ILogger<OrganisationService> logger)
        {
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<IList<SiteModel>> GetSitesForCustomer(string customerId)
        {
            IList<Site> sites = new List<Site>();
            IList<SiteModel> returnedMappedSites = null;
            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites?customerId={customerId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    IList<Site> returnedSites = JsonConvert.DeserializeObject<IList<Site>>(reply);
                    returnedMappedSites = _mapper.Map<IList<SiteModel>>(returnedSites);
                }
                else
                {
                    _logger.LogError("GetSitesForCustomer failed with error: " + reply);
                    throw new Exception("Error getting sites for customerId. Response code from downstream: " + response.StatusCode); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetSitesForCustomer: " + e.Message);
                throw new Exception(e.Message);;
            }
            return returnedMappedSites;
        }

        public async Task<SiteModel> GetSite(string siteId, string customerId)
        {
            IList<Site> sites = new List<Site>();
            SiteModel returnedMappedSite = null;
            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var includeCustomerId = "";
                if (customerId != null) includeCustomerId = $"?customerId={customerId}";
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}{includeCustomerId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    Site returnedSite = JsonConvert.DeserializeObject<Site>(reply);
                    returnedMappedSite = _mapper.Map<SiteModel>(returnedSite);
                }
                else
                {
                    _logger.LogError("GetSite failed with error: " + reply);
                    throw new Exception("Error getting site. Response code from downstream: " + response.StatusCode); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetSite: " + e.Message);
                throw new Exception(e.Message);;
            }
            return returnedMappedSite;
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
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var response = await _httpClient.DeleteAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var deletedSite = JsonConvert.DeserializeObject<Site>(reply);
                    mappedDeletedSite = _mapper.Map<SiteModel>(deletedSite);
                }
                else
                {
                    _logger.LogError("DeleteSite failed with error: " + reply);
                    throw new Exception($"Error deleting a site. {response.StatusCode} Response code from downstream: " + reply); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("DeleteSite: " + e.Message);
                throw new Exception(e.Message);;
            }
            return mappedDeletedSite;
        }

        private async Task<SiteModel> SaveSite(SiteModel newSite, bool isUpdate = false)
        {
            SiteModel returnedMappedSite = null;
            Site mappedSite = _mapper.Map<Site>(newSite);            

            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var payload =JsonConvert.SerializeObject(mappedSite);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                if (!isUpdate)
                    response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites", httpContent);
                else
                    response = await _httpClient.PutAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites/{newSite.SiteId}", httpContent);

                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedSite = JsonConvert.DeserializeObject<Site>(reply);
                    returnedMappedSite = _mapper.Map<SiteModel>(returnedSite);
                }
                else
                {
                    _logger.LogError("SaveSite failed with error: " + reply);
                    throw new Exception($"Error saving a site. {response.StatusCode} Response code from downstream: " + reply); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("SaveSite: " + e.Message);
                throw new Exception(e.Message);;
            }
            return returnedMappedSite;
        }
    }
}