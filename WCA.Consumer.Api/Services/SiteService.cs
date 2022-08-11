using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
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
                    var returnedSites = JsonConvert.DeserializeObject<IList<Site>>(reply);
                    returnedMappedSites = _mapper.Map<IList<SiteModel>>(returnedSites);
                }
                else
                {
                    _logger.LogError("GetOrganisationOverview failed with error: " + reply);
                    throw new Exception("Error getting org overview. Response code from downstream: " + response.StatusCode); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetOrganisationOverview: " + e.Message);
                throw new Exception(e.Message);;
            }
            return returnedMappedSites;
            // IList<Site> sites = new List<Site>();
            // return GetAllSites();
        }

        public SiteModel GetSite(string siteId)
        {
            if (siteId != null)
            {
                GeoLocation geoLocation = new GeoLocation {
                    Latitude = -33.71218,
                    Longitude = 150.95753
                };
                SiteLocationModel location = new SiteLocationModel {
                    Id = "AU/GEO/p0/12161",
                    Address = "Kellyville, Sydney, New South Wales",
                    GeoLocation = geoLocation
                };
                SiteModel site = new SiteModel {
                    SiteId = "187c1bdd-8efe-493d-b9c3-3a4f027e0940",
                    Name = "Kellyville",
                    CustomerId = "manual-test-customer-id",
                    Location = location,
                    CreatedAt = 1651710340528
                };
                return site;
            }
            else
            {
                return null;
            }
        }

        private IList<SiteModel> GetAllSites()
        {
            IList<SiteModel> sites = new List<SiteModel>();

            SiteModel site = CreateSite(-33.71218, 150.95753, "AU/GEO/p0/12161", "John Scott Park, Samford Village QLD",
                                "187c1bdd-8efe-493d-b9c3-3a4f027e0940", "John Scott Park", "manual-test-customer-id", 1651710340528);
            
            sites.Add(site);

            site = CreateSite(-33.85754, 151.181, "AU/GEO/p0/11073", "Balmain, Sydney, New South Wales",
                                "91126d4d-c12b-40a3-a58b-9fbdda73e9e5", "telstra-balmain-design-centre", "manual-test-customer-id", 1651710480031);
            
            sites.Add(site);

            site = CreateSite(-34.41835, 150.90239, "AU/STR/p0/375083", "Cliff Road, Wollongong, New South Wales",
                                "bceead95-5b9d-47bc-9d93-4740db6c1292", "Blue Mile Area Wollongong", "manual-test-customer-id", 1655347904941);
            
            sites.Add(site);
            return sites;
        }

        public async Task<SiteModel> CreateSite(SiteModel newSite)
        {
            SiteModel returnedMappedSite = null;
            Site mappedSite = _mapper.Map<Site>(newSite);            

            if (newSite.Metadata.Tags.Count > 0)
            {         
                mappedSite.Tags = new List<SiteTag>();
                foreach(var tagItem in newSite.Metadata.Tags)
                {
                    SiteTag siteTag = new SiteTag {
                        SiteId = mappedSite.SiteId,
                        Tag = new Tag {
                            Name = tagItem.Key,
                            Value = tagItem.Value[0]
                        }
                    };
                    mappedSite.Tags.Add(siteTag);
                }
            }

            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var payload =JsonConvert.SerializeObject(mappedSite);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/sites", httpContent);
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedSite = JsonConvert.DeserializeObject<Site>(reply);
                    returnedMappedSite = _mapper.Map<SiteModel>(returnedSite);
                    
                    // re-map the tags
                    if (returnedSite.Tags != null && returnedSite.Tags.Count > 0)
                    {         
                        Dictionary<string, string[]> tags = new Dictionary<string, string[]>();
                        foreach(var tagItem in returnedSite.Tags)
                        {
                            tags.Add(tagItem.Tag.Name, new []{ tagItem.Tag.Value });
                        }
                        returnedMappedSite.Metadata.Tags = tags; 
                    }
                }
                else
                {
                    _logger.LogError("CreateSite failed with error: " + reply);
                    throw new Exception($"Error creating a site. {response.StatusCode} Response code from downstream: " + reply); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CreateSite: " + e.Message);
                throw new Exception(e.Message);;
            }
            return returnedMappedSite;
        }

        public SiteModel UpdateSite(string siteId, SiteModel site)
        {
            return site;
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

        private SiteModel CreateSite(double latitude, double longitude, string siteLocationId, string siteAddress,
                                string siteId, string siteName, string customerId, long createdAt)
        {
            GeoLocation geoLocation = new GeoLocation {
                Latitude = latitude,
                Longitude = longitude
            };
            SiteLocationModel location = new SiteLocationModel {
                Id = siteLocationId,
                Address = siteAddress,
                GeoLocation = geoLocation
            };
            SiteModel site = new SiteModel {
                SiteId = siteId,
                Name = siteName,
                CustomerId = customerId,
                Location = location,
                CreatedAt = createdAt
            };
            return site;
        }
    }
}