using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telstra.Common;
using WCA.Consumer.Api.Helpers;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Services
{
    public class DeviceManagementService : IDeviceManagementService
    {
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly ILogger _logger;

        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime;
        private readonly string _errUnauthorizedUser = "User not authorized to access device!";
        private readonly string _errNoAccessToken = "Device Management access token not received!";

        public DeviceManagementService(IRestClient restClient, 
                                        AppSettings appSettings, 
                                        ILogger<DeviceManagementService> logger,
                                        IMemoryCache cache)
        {
            _httpClient = restClient;
            _appSettings = appSettings;
            _logger = logger;
            _cache = cache;

            _shortCacheTime = DateTimeOffset.Now.AddSeconds(_appSettings.ShortCacheTime);
        }

        public async Task<RtspFeedModel> GetRtspFeed(string authorisationEmail, string edgeDeviceId, string leafDeviceId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            try
            {
                bool isLeafDeviceMapped = false;
                var accessToken = string.Empty;

                var cacheKeyLeafDeviceMapped = $"{nameof(IsLeafDeviceMapped)}-{authorisationEmail}-{edgeDeviceId}-{leafDeviceId}";
                var cacheKeyAccessToken = $"{nameof(GetAccessToken)}-{authorisationEmail}-{edgeDeviceId}-{leafDeviceId}";
                var cachedValue = _cache.Get(cacheKeyLeafDeviceMapped);

                if (cachedValue == null)
                {
                    var tenantOverview = await GetTenantOverview(authorisationEmail);
                    isLeafDeviceMapped = IsLeafDeviceMapped(tenantOverview, edgeDeviceId, leafDeviceId);
                    accessToken = await GetAccessToken();

                    _ = Task.Run(() => 
                    {
                        _cache.Set<bool>(cacheKeyLeafDeviceMapped, isLeafDeviceMapped, _shortCacheTime);
                        _cache.Set<string>(cacheKeyAccessToken, accessToken, _shortCacheTime);
                        _logger.LogTrace($"{nameof(GetRtspFeed)} cache set");
                    });
                }
                else
                {
                    isLeafDeviceMapped = (bool)cachedValue;
                    accessToken = _cache.Get<string>(cacheKeyAccessToken);
                }

                if (!isLeafDeviceMapped)
                {
                    throw new Exception(_errUnauthorizedUser);
                }

                return await GetRtspFeedFromDm(accessToken, edgeDeviceId, leafDeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRtspFeed: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task<TenantOverview> GetTenantOverview(string authorisationEmail)
        {
            try
            {
                var tenantOvervireRequest = new HttpRequestMessage(HttpMethod.Get,
                        $"{_appSettings.StorageAppHttp.BaseUri}/organisations/overview?email={authorisationEmail}&withHealthStatus=false");
                var tenantOverview = await _httpClient.SendAsync<TenantOverview>(tenantOvervireRequest,
                                                CancellationToken.None) ?? throw new Exception("User not found!");

                return tenantOverview;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetTenantOverview: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private bool IsLeafDeviceMapped(TenantOverview tenantOverview, string edgeDeviceId, string leafDeviceId)
        {
            try
            {
                var leafDeviceMapped = false;

                foreach (var site in tenantOverview.Sites)
                {
                    foreach (var edgeDevice in site.EdgeDevices)
                    {
                        foreach (var leafDevice in edgeDevice.LeafDevices)
                        {
                            if (edgeDevice.EdgeDeviceId == edgeDeviceId && leafDevice.LeafId == leafDeviceId)
                            {
                                leafDeviceMapped = true;
                                break;
                            }
                        }
                    }
                }

                return leafDeviceMapped;
            }
            catch (Exception ex)
            {
                _logger.LogError("IsLeafDeviceMapped: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        // TODO: consider moving into separate IoT-SC client (similar to previous approach).
        private async Task<string> GetAccessToken()
        {
            try
            {
                var accessTokenRequest = new HttpRequestMessage(
                                                HttpMethod.Post, 
                                                $"{_appSettings.DeviceManagementCredentials.BaseUri}" +
                                                $"{_appSettings.DeviceManagementCredentials.AccessTokenUri}");

                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("client_id", _appSettings.DeviceManagementCredentials.ClientId));
                collection.Add(new("client_secret", _appSettings.DeviceManagementCredentials.ClientSecret));
                collection.Add(new("grant_type", _appSettings.DeviceManagementCredentials.GrantType));
                var content = new FormUrlEncodedContent(collection);
                accessTokenRequest.Content = content;
                var accessToken = await _httpClient.SendAsync<AccessTokenModel>(accessTokenRequest, CancellationToken.None)
                                        ?? throw new Exception(_errNoAccessToken);

                return accessToken.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAccessToken: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task<RtspFeedModel> GetRtspFeedFromDm(string accessToken, string edgeDeviceId, string leafDeviceId)
        {
            try
            {
                var rtspFeedRequest = new HttpRequestMessage(
                                            HttpMethod.Post,
                                            $"{_appSettings.DeviceManagementCredentials.BaseUri}" +
                                            $"{string.Format(_appSettings.DeviceManagementCredentials.RtspFeedUri, edgeDeviceId)}");
                rtspFeedRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                var requestPayload = new RtspFeedRequestModel();
                requestPayload.RequestParameters.Payload.ModuleName = leafDeviceId;
                rtspFeedRequest.Content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json");
                var rtspFeed = await _httpClient.SendAsync<RtspFeedModel>(rtspFeedRequest, CancellationToken.None);

                return rtspFeed;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetRtspFeedFromDm: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
