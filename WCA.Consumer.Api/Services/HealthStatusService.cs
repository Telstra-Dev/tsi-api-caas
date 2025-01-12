using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using Telstra.Core.Data.Models;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WCA.Consumer.Api.Services
{
    public class HealthStatusService : IHealthStatusService
    {
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDeviceService _deviceService;
        private readonly ISiteService _siteService;
        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime = DateTimeOffset.Now.AddSeconds(60);
        private readonly int _deviceRecentlyOnlineMaxMinutes;
        private readonly int _deviceRecentlySentTelemetryMaxMinutes;

        public HealthStatusService(IRestClient httpClient,
                        AppSettings appSettings,
                        IMapper mapper,
                        ILogger<OrganisationService> logger,
                        IDeviceService deviceService,
                        ISiteService siteService,
                        IMemoryCache cache)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
            _deviceService = deviceService;
            _siteService = siteService;
            _cache = cache;
            _deviceRecentlyOnlineMaxMinutes = _appSettings.DeviceRecentlyOnlineMaxMinutes;
            _deviceRecentlySentTelemetryMaxMinutes = _appSettings.DeviceRecentlySentTelemetryMaxMinutes;
        }
        public async Task<HealthStatusModel> GetHealthStatusFromSiteId(string authorisationEmail, string siteId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            var siteModel = await _siteService.GetSite(authorisationEmail, siteId, null);
            if (siteModel == null)
            {
                return null;
            }
            var site = _mapper.Map<Site>(siteModel);

            return await GetSiteHealthStatus(authorisationEmail, site);
        }

        public async Task<HealthStatusModel> GetSiteHealthStatus(string authorisationEmail, Site site)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            var siteStatus = await GetSiteStatus(authorisationEmail, site.SiteId);
            var siteHealth = siteStatus.GetHealthStatus();

            return siteHealth;
        }

        public async Task<HealthStatusModel> GetHealthStatusFromDeviceId(string authorisationEmail, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            var deviceModel = await _deviceService.GetDevice(authorisationEmail, deviceId, null);
            if (deviceModel == null)
            {
                return null;
            }
            var device = _mapper.Map<Device>(deviceModel);

            return await GetDeviceHealthStatus(authorisationEmail, device);
        }

        public async Task<HealthStatusModel> GetDeviceHealthStatus(string authorisationEmail, Device device)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.

            if (device.Type == DeviceType.gateway.ToString())
            {
                return await GetEdgeDeviceHealthStatus(authorisationEmail, device);
            }
            else
            {
                return await GetLeafDeviceHealthStatus(device);
            }
        }

        private async Task<HealthStatusModel> GetEdgeDeviceHealthStatus(string authorisationEmail, Device device)
        {
            // Check cache
            var cacheKey = $"{nameof(GetEdgeDeviceHealthStatus)}-{device.DeviceId}";
            var cacheValue = (HealthStatusModel)_cache.Get(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            var edgeDeviceStatus = await GetEdgeDeviceStatus(authorisationEmail, device);
            var edgeDeviceHealth = edgeDeviceStatus.GetHealthStatus();

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, edgeDeviceHealth, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetEdgeDeviceHealthStatus)} cache set");
            });

            // Priority 6: Assume asset is online.
            return edgeDeviceHealth;
        }

        private async Task<HealthStatusModel> GetLeafDeviceHealthStatus(Device device)
        {
            //TODO: Sunil - This status is spoofed
            // Check cache
            var cacheKey = $"{nameof(GetLeafDeviceHealthStatus)}-{device.DeviceId}";
            var cacheValue = (HealthStatusModel)_cache.Get(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            var edgeDeviceLastHealthReadingTimestamp = "";
            var edgeDeviceIsOnline = true;

            var leafDeviceStatus = await GetLeafDeviceStatus(device, edgeDeviceIsOnline);
            var health = leafDeviceStatus.GetHealthStatus();

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, health, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetLeafDeviceHealthStatus)} cache set");
            });

            return health;
        }

        // public TenantOverview GetTenantHealthStatus(TenantOverview overview)
        public async Task<TenantOverview> GetTenantHealthStatus(string authorisationEmail, TenantOverview overview)
        {
            if (string.IsNullOrWhiteSpace(authorisationEmail))
            {
                throw new Exception($"[ValidationError] No authorisationEmail specified.");
            }

            // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.

            foreach (var site in overview.Sites)
            {
                var edgeDevicesStatus = new List<EdgeDeviceStatus>();

                foreach (var edgeDevice in site.EdgeDevices)
                {
                    var leafDevicesStatus = new List<LeafDeviceStatus>();

                    foreach (var leafDevice in edgeDevice.LeafDevices)
                    {
                        var leafDeviceStatus = new LeafDeviceStatus()
                        {
                            LeafDeviceId                            = leafDevice.LeafId,
                            EdgeDeviceId                            = edgeDevice.EdgeDeviceId,
                            LastHealthReadingTimestamp              = leafDevice.LastActiveTime,
                            LastTelemetryReadingTimestamp           = leafDevice.LastTelemetryTime ?? 0,
                            RequiresConfiguration                   = leafDevice.RequiresConfiguration ?? true,
                            // EdgeDeviceIsOnline                      = await CheckDeviceRecentlyOnline(edgeDevice.EdgeDeviceId, _deviceRecentlyOnlineMaxMinutes, "edgedevice"),
                            EdgeDeviceIsOnline                      = EdgeDeviceStatus.CheckEdgeDeviceRecentlyOnline(edgeDevice.LastActiveTime, _deviceRecentlyOnlineMaxMinutes),
                            DeviceRecentlyOnlineMaxMinutes          = _deviceRecentlyOnlineMaxMinutes,
                            DeviceRecentlySentTelemetryMaxMinutes   = _deviceRecentlySentTelemetryMaxMinutes,
                        };
                        leafDevicesStatus.Add(leafDeviceStatus);
                        leafDevice.HealthStatus = leafDeviceStatus.GetHealthStatus();
                    }

                    var edgeDeviceStatus = new EdgeDeviceStatus()
                    {
                        EdgeDeviceId                            = edgeDevice.EdgeDeviceId,
                        LastHealthReadingTimestamp              = edgeDevice.LastActiveTime,
                        LeafDevices                             = leafDevicesStatus,
                        DeviceRecentlyOnlineMaxMinutes          = _deviceRecentlyOnlineMaxMinutes,
                    };
                    edgeDevicesStatus.Add(edgeDeviceStatus);
                    edgeDevice.HealthStatus = edgeDeviceStatus.GetHealthStatus();
                }

                var siteStatus = new SiteStatus()
                {
                    EdgeDevices                             = edgeDevicesStatus,
                };
                site.HealthStatus = siteStatus.GetHealthStatus();
            }

            // Less-optimal approach
            // foreach (var site in overview.Sites)
            // {
            //     foreach (var edgeDevice in site.EdgeDevices)
            //     {
            //         foreach (var leafDevice in edgeDevice.LeafDevices)
            //         {
            //             leafDevice.HealthStatus = await GetHealthStatusFromDeviceId(leafDevice.LeafId);
            //         }
            //         edgeDevice.HealthStatus = await GetHealthStatusFromDeviceId(edgeDevice.EdgeDeviceId);
            //     }
            //     site.HealthStatus = await GetHealthStatusFromSiteId(site.SiteId);
            // }

            return overview;
        }

        private async Task<LeafDeviceStatus> GetLeafDeviceStatus(Device leafDevice, bool edgeDeviceIsOnline)
        {
            //TODO: Sunil - This status method is spoofed
            var leafDeviceStatus = new LeafDeviceStatus()
            {
                LeafDeviceId                            = leafDevice.DeviceId,
                EdgeDeviceId                            = leafDevice.EdgeDeviceId,
                LastHealthReadingTimestamp              = "",
                LastTelemetryReadingTimestamp           = 0,
                RequiresConfiguration                   = false,
                EdgeDeviceIsOnline                      = edgeDeviceIsOnline,
                DeviceRecentlyOnlineMaxMinutes          = _deviceRecentlyOnlineMaxMinutes,
                DeviceRecentlySentTelemetryMaxMinutes   = _deviceRecentlySentTelemetryMaxMinutes,
            };

            return leafDeviceStatus;
        }

        private async Task<EdgeDeviceStatus> GetEdgeDeviceStatus(string authorisationEmail, Device edgeDevice)
        {
            //TODO: Sunil - This status method is spoofed
            
            var edgeDeviceLastHealthReadingTimestamp = "";
            var edgeDeviceIsOnline = true;

            // Check leaf devices
            var leafDevices = Array.Empty<Device>();
            var leafDevicesStatus = new List<LeafDeviceStatus>();

            foreach (var leafDevice in leafDevices)
            {
                var leafDeviceStatus = await GetLeafDeviceStatus(leafDevice, edgeDeviceIsOnline);
                leafDevicesStatus.Add(leafDeviceStatus);
            }

            var edgeDeviceStatus = new EdgeDeviceStatus()
            {
                EdgeDeviceId                            = edgeDevice.EdgeDeviceId,
                LastHealthReadingTimestamp              = "",
                LeafDevices                             = leafDevicesStatus,
                DeviceRecentlyOnlineMaxMinutes          = _deviceRecentlyOnlineMaxMinutes,
            };

            return edgeDeviceStatus;
        }

        private async Task<SiteStatus> GetSiteStatus(string authorisationEmail, string siteId)
        {
            var edgeDevices = (await _deviceService.GetDevices(authorisationEmail, null, siteId))
                .Cast<DeviceModel>()
                .Where(d => d.Type == DeviceType.gateway)
                .Select(d => _mapper.Map<Device>(d));
            var edgeDevicesStatus = new List<EdgeDeviceStatus>();

            foreach (var edgeDevice in edgeDevices)
            {
                var edgeDeviceStatus = await GetEdgeDeviceStatus(authorisationEmail, edgeDevice);
                edgeDevicesStatus.Add(edgeDeviceStatus);
            }

            var siteStatus = new SiteStatus()
            {
                EdgeDevices = edgeDevicesStatus,
            };

            return siteStatus;
        }

        // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.
        private async Task<string?> GetDeviceLastOnlineTimestamp(string deviceId, string type)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/healthStatus?deviceId={deviceId}");
                var returnedHealthDataStatus = await _httpClient.SendAsync<HealthDataStatus>(request, CancellationToken.None);
                if (returnedHealthDataStatus != null && returnedHealthDataStatus.EdgeStarttime != null)
                {
                    return returnedHealthDataStatus.EdgeStarttime.ToString();
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("GetDeviceLastOnlineTimestamp: " + e.Message);
            }

            return null;
        }

        // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.
        private async Task<long?> GetLeafDeviceLastOnlineTimestamp(string leafDeviceId, string edgeDeviceId)
        {
            try
            {
                // Return true if the camera's last telemetry timestamp is not NULL and is strictly within `maxMinutes` (>= maxMinutes is considered data offline).
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{edgeDeviceId}/leafDevices/{leafDeviceId}/latestTelemetryStatus");
                var returnedStatus = await _httpClient.SendAsync<LeafDeviceLatestTelemetryStatus>(request, CancellationToken.None);
                if (returnedStatus != null && returnedStatus.Timestamp != null)
                {
                    return (long)returnedStatus.Timestamp;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("CheckCameraRecentlySentTelemetry: " + e.Message);
            }

            return null;
        }

        // TODO: Add RBAC checks. Need to prevent users from modifying objects outside their tenant.
        private async Task<bool> GetLeafDeviceRequiresConfiguration(string leafDeviceId, string edgeDeviceId)
        {
            try
            {
                // Return true if camera has at least one associated tripline or polygon.
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{edgeDeviceId}/leafDevices/{leafDeviceId}/configurationStatus");
                var status = await _httpClient.SendAsync<LeafDeviceConfigurationStatus>(request, CancellationToken.None);
                if (status != null)
                {
                    return status.RequiresConfiguration;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("GetLeafDeviceRequiresConfiguration: " + e.Message);
            }

            return true;
        }
    }
}