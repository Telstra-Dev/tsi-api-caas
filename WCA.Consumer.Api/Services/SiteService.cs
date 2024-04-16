using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading;
using System.Linq;
using Flurl;
using Flurl.Http;

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
                        // Id = x.Address.Id,
                        Address = x.Address,
                        GeoLocation = new GeoLocation 
                        {
                            Longitude = x.Address.Longitude,
                            Latitude = x.Address.Latitude
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

        public async Task<SiteTelemetryProperty> GetSiteTelProperties(string authorisationEmail, int siteId)
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

        public async Task<SiteModel> GetSiteById(string authorisationEmail, int siteId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                var site =  await $"{_appSettings.StorageAppHttp.BaseUri}/sites/{siteId}"
                                    .AppendQueryParam("email", authorisationEmail)
                                    .AllowAnyHttpStatus()
                                    .GetJsonAsync<SiteNameModel>();
                
                if (site != null)
                {
                    return new SiteModel 
                    {
                        SiteId = site.Id,
                        Name = site.DisplayName,
                        Metadata = new SiteMetadata 
                        {
                            Tags = site.Tags
                        },
                        Location = new SiteLocationModel
                        {
                            // Id = site.Address.Id,
                            Address = site.Address,
                            GeoLocation = new GeoLocation
                            {
                                Longitude = site.Address?.Longitude,
                                Latitude = site.Address?.Latitude
                            }
                        }
                    };
                }

                throw new Exception("Site not found");
            }
            catch (Exception ex)
            {
                var errorMessage = $"GetSiteById: {ex.Message}";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public async Task<int> CreateOrUpdateSite(string authorisationEmail, SiteModel site)
        {
            try
            {
                var siteNameModel = new SiteNameModel
                {
                    Id = site.SiteId,
                    DisplayName = site.Name,
                    TagName = site.Metadata.TagName,
                    Tags = site.Metadata.Tags,
                    Address = new SiteAddress
                    {
                        // Id = site.Location.Address.Id,
                        Name = site.Location.Address.Name,
                        StreetNumber = site.Location.Address.StreetNumber,
                        StreetName = site.Location.Address.StreetName,
                        Suburb = site.Location.Address.Suburb,
                        Postcode = site.Location.Address.Postcode,
                        State= site.Location.Address.State,
                        Country =  site.Location.Address.Country,
                        // SvNote = site.Location.Address.SvNote,
                        Longitude = site.Location.GeoLocation?.Longitude?.ToString(),
                        Latitude = site.Location.GeoLocation?.Latitude?.ToString()
                    }
                };

                if (site.SiteId == 0)
                {
                    return await $"{_appSettings.StorageAppHttp.BaseUri}/sites"
                        .AllowAnyHttpStatus()
                        .AppendQueryParam("email", authorisationEmail)
                        .PostJsonAsync(siteNameModel)
                        .ReceiveJson<int>();
                }
                else
                {
                    return await $"{_appSettings.StorageAppHttp.BaseUri}/sites/{site.SiteId}"
                        .AllowAnyHttpStatus()
                        .AppendQueryParam("email", authorisationEmail)
                        .PutJsonAsync(siteNameModel)
                        .ReceiveJson<int>();
                    
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Fail to SaveSite: " + e.Message);
                throw new Exception(e.Message);
            }
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
    }
}