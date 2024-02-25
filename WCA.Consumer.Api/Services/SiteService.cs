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

        public async Task<IList<SiteModel>> GetSites(string authorisationEmail)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var foundSites = await _httpClient.GetAsync<IList<SiteNameModel>>($"{_appSettings.StorageAppHttp.BaseUri}/sites/names?email={authorisationEmail}", CancellationToken.None);

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
                        //Address = $"{x.Address.SvNote}, {x.Address.State}, {x.Address.Country}",
                        Address = x.Address,
                        GeoLocation = new GeoLocation 
                        {
                            Longitude = Convert.ToDouble(x.Address.Longitude),
                            Latitude = Convert.ToDouble(x.Address.Latitude)
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

        // TODO: consider removing this API - doesn't appear compatible with auth strategy.
        public async Task<IList<SiteModel>> GetSitesForCustomer(string authorisationEmail, string customerId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

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

        public async Task<SiteTelemetryProperty> GetSiteTelProperties(string authorisationEmail, string siteId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var downstreamResult = await _httpClient.GetAsync<SiteTelemetryProperty>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}/locations?email={authorisationEmail}", CancellationToken.None);

                return downstreamResult;

            }
            catch (Exception e)
            {
                _logger.LogError("Get site location error: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        // TODO: consider removing this API - doesn't appear compatible with auth strategy.
        public async Task<SiteModel> GetSite(string authorisationEmail, string siteId, string customerId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

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

        public async Task<SiteModel> GetSiteById(string authorisationEmail, string siteId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                SiteModel foundMappedSite = null;
                var foundSite = await _httpClient.GetAsync<SiteNameModel>(
                                                    $"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}?email={authorisationEmail}",
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
                            //Address = $"{foundSite.Address.SvNote}, {foundSite.Address.State}, {foundSite.Address.Country}",
                            Address = foundSite.Address,
                            GeoLocation = new GeoLocation
                            {
                                Longitude = Convert.ToDouble(foundSite.Address.Longitude),
                                Latitude = Convert.ToDouble(foundSite.Address.Latitude)
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

        public async Task<bool> CreateSite(string authorisationEmail, SiteModel newSite)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.

            return await SaveSite(newSite, authorisationEmail);
        }

        public async Task<bool> UpdateSite(string authorisationEmail, string siteId, SiteModel updateSite)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.

            return await SaveSite(updateSite, authorisationEmail, true);
        }

        public async Task<bool> DeleteSite(string authorisationEmail, string siteId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var deletedSite = await _httpClient.DeleteAsync<bool>(
                                                        $"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}?email={authorisationEmail}",
                                                        CancellationToken.None);

                return deletedSite;

            }
            catch (Exception e)
            {
                _logger.LogError("Fail to DeleteSite: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        private async Task<bool> SaveSite(SiteModel newSite, string authorisationEmail, bool isUpdate = false)
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
                        Name = newSite.SiteId,
                        StreetNumber = newSite.Location.Address.StreetNumber,
                        StreetName = newSite.Location.Address.StreetName,
                        Suburb = newSite.Location.Address.Suburb,
                        Postcode = newSite.Location.Address.Postcode,
                        State= newSite.Location.Address.State,
                        Country =  newSite.Location.Address.Country,
                        SvNote = newSite.Location.Address.SvNote,
                        Longitude = newSite.Location.GeoLocation?.Longitude.ToString(),
                        Latitude = newSite.Location.GeoLocation?.Latitude.ToString()
                    }
                };

                var payload = JsonConvert.SerializeObject(siteNameModel);

                if (!isUpdate)
                {
                    return await _httpClient.PostAsync<bool>($"{_appSettings.StorageAppHttp.BaseUri}/sites?email={authorisationEmail}", payload, CancellationToken.None);
                }
                else
                {
                    return await _httpClient.PutAsync<bool>($"{_appSettings.StorageAppHttp.BaseUri}/sites/{newSite.SiteId}?email={authorisationEmail}", payload, CancellationToken.None);
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