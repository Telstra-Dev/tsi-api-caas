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

                return foundSites.Select(x => new SiteModel 
                { 
                    SiteId = x.Id, 
                    Name = x.DisplayName,
                    Metadata = new SiteMetadata 
                    {
                        Tags = x.Tags
                    },
                    Location = new SiteLocationModel
                    {
                        Id = Convert.ToString(x.Address.Id),
                        Address = $"{x.Address.SvNote}, {x.Address.State}, {x.Address.Country}",
                        GeoLocation = new GeoLocation 
                        {
                            Longitude = Convert.ToDouble(x.Address.Sites[0].Longitude),
                            Latitude = Convert.ToDouble(x.Address.Sites[0].Latitude)
                        }
                    }
                }).ToList();
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

        public async Task<SiteModel> GetSiteById(string siteId, string token)
        {
            try
            {
                var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
                if (string.IsNullOrEmpty(emailFromToken))
                    throw new NullReferenceException("Invalid claim from token.");

                SiteModel foundMappedSite = null;
                var foundSite = await _httpClient.GetAsync<SiteNameModel>(
                                                    $"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}?email={emailFromToken}",
                                                    CancellationToken.None);
                if (foundSite != null)
                {
                    foundMappedSite = new SiteModel 
                    {
                        SiteId = foundSite.Id,
                        Name = foundSite.DisplayName,
                        Metadata = new SiteMetadata 
                        {
                            Tags = foundSite.Tags
                        },
                        Location = new SiteLocationModel
                        {
                            Id = Convert.ToString(foundSite.Address.Id),
                            Address = $"{foundSite.Address.SvNote}, {foundSite.Address.State}, {foundSite.Address.Country}",
                            GeoLocation = new GeoLocation
                            {
                                Longitude = Convert.ToDouble(foundSite.Address.Sites[0].Longitude),
                                Latitude = Convert.ToDouble(foundSite.Address.Sites[0].Latitude)
                            }
                        }
                    };
                }

                return foundMappedSite;
            }
            catch (Exception ex)
            {
                var errorMessage = $"GetSiteById: {ex.Message}";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public async Task<bool> CreateSite(SiteModel newSite)
        {
            return await SaveSite(newSite);
        }

        public async Task<bool> UpdateSite(string siteId, SiteModel updateSite)
        {
            return await SaveSite(updateSite, true);
        }

        public async Task<bool> DeleteSite(string siteId, string token)
        {
            try
            {
                var emailFromToken = TokenClaimsHelper.GetEmailFromToken(token);
                if (string.IsNullOrEmpty(emailFromToken))
                    throw new NullReferenceException("Invalid claim from token.");

                var deletedSite = await _httpClient.DeleteAsync<bool>(
                                                        $"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}?email={emailFromToken}",
                                                        CancellationToken.None);

                return deletedSite;

            }
            catch (Exception e)
            {
                _logger.LogError("Fail to DeleteSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        private async Task<bool> SaveSite(SiteModel newSite, bool isUpdate = false)
        {
            try
            {
                var siteNameModel = new SiteNameModel 
                {
                    Id = newSite.SiteId,
                    DisplayName = newSite.Name,
                    Tags = newSite.Metadata.Tags,
                    Address = new SiteAddress
                    {
                        Id = Convert.ToInt32(newSite.Location.Id),
                        SvNote = newSite.Location.Address,
                        Sites = new List<NameModelSite> 
                        {
                            new NameModelSite
                            {
                                Longitude = Convert.ToString(newSite.Location.GeoLocation.Longitude),
                                Latitude = Convert.ToString(newSite.Location.GeoLocation.Latitude)
                            }
                        }
                    }
                };

                var payload = JsonConvert.SerializeObject(siteNameModel);

                if (!isUpdate)
                {
                    return await _httpClient.PostAsync<bool>($"{_appSettings.StorageAppHttp.BaseUri}/sites", payload, CancellationToken.None);
                }
                else
                {
                    return await _httpClient.PutAsync<bool>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{newSite.SiteId}", payload, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to SaveSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }
    }
}