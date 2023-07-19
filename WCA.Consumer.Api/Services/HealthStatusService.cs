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
        }
        public async Task<HealthStatusModel> GetHealthStatusFromSiteId(string siteId)
        {
            var siteModel = await _siteService.GetSite(siteId, null);
            if (siteModel == null)
            {
                return null;
            }
            var site = _mapper.Map<Site>(siteModel);

            return await GetSiteHealthStatus(site);
        }

        public async Task<HealthStatusModel> GetSiteHealthStatus(Site site)
        {
            var health = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Site online",
                Action = "Expand site to review",
            };

            var gatewayModels = (await _deviceService.GetDevices(null, site.SiteId))
                                .Cast<DeviceModel>()
                                .Where(d => d.Type == DeviceType.gateway);

            if (gatewayModels == null || gatewayModels.Count() == 0)
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "No gateways";
                health.Action = "Configure in site menu";
            }

            foreach (var gatewayModel in gatewayModels)
            {
                var device = _mapper.Map<Device>(gatewayModel);
                var deviceHealth = await GetEdgeDeviceHealthStatus(device);

                // Prioritise RED, keep looping if AMBER to catch any RED
                if (deviceHealth.Code == HealthStatusCode.RED)
                {
                    health.Code = deviceHealth.Code;
                    health.Reason = deviceHealth.Reason;
                    health.Action = "Expand site to review";

                    return health;
                }
                else if (deviceHealth.Code == HealthStatusCode.AMBER)
                {
                    health.Code = deviceHealth.Code;
                    health.Reason = deviceHealth.Reason;
                    health.Action = "Expand site to review";
                }
            }

            return health;
        }

        public async Task<HealthStatusModel> GetHealthStatusFromDeviceId(string deviceId)
        {
            var deviceModel = await _deviceService.GetDevice(deviceId, null);
            if (deviceModel == null)
            {
                return null;
            }
            var device = _mapper.Map<Device>(deviceModel);

            return await GetDeviceHealthStatus(device);
        }

        public async Task<HealthStatusModel> GetDeviceHealthStatus(Device device)
        {
            if (device.Type == DeviceType.gateway.ToString())
            {
                return await GetEdgeDeviceHealthStatus(device);
            }
            else
            {
                return await GetCameraHealthStatus(device);
            }
        }

        private async Task<HealthStatusModel> GetEdgeDeviceHealthStatus(Device device)
        {
            var edgeDeviceHealth = new HealthStatusModel
            {
                Code = HealthStatusCode.GREEN,
                Reason = "Gateway online",
                Action = "Expand gateway to review",
            };

            // Check cache
            var cacheKey = $"{nameof(GetEdgeDeviceHealthStatus)}-{device.DeviceId}";
            var cacheValue = (HealthStatusModel)_cache.Get(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            // Check online status
            if (!await CheckDeviceRecentlyOnline(device.DeviceId, _deviceRecentlyOnlineMaxMinutes, "edgedevice"))
            {
                edgeDeviceHealth.Code = HealthStatusCode.RED;
                edgeDeviceHealth.Reason = "Gateway offline";
                edgeDeviceHealth.Action = "Contact support";
            }
            else
            {
                // Check leaf devices
                var leafDevices = await _deviceService.GetLeafDevicesForGateway(device.DeviceId);

                if (leafDevices == null || leafDevices.Count == 0)
                {
                    edgeDeviceHealth.Code = HealthStatusCode.AMBER;
                    edgeDeviceHealth.Reason = "No cameras";
                    edgeDeviceHealth.Action = "Configure in gateway menu";
                }


                foreach (var leafDevice in leafDevices)
                {
                    var deviceHealth = await GetCameraHealthStatus(leafDevice);

                    // Prioritise RED, keep looping if AMBER to catch any RED
                    if (deviceHealth.Code == HealthStatusCode.RED)
                    {
                        edgeDeviceHealth.Code = deviceHealth.Code;
                        edgeDeviceHealth.Reason = deviceHealth.Reason;
                        edgeDeviceHealth.Action = "Expand gateway to review";

                        break;
                    }
                    else if (deviceHealth.Code == HealthStatusCode.AMBER)
                    {
                        edgeDeviceHealth.Code = deviceHealth.Code;
                        edgeDeviceHealth.Reason = deviceHealth.Reason;
                        edgeDeviceHealth.Action = "Expand gateway to review";
                    }
                }
            }

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, edgeDeviceHealth, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetEdgeDeviceHealthStatus)} cache set");
            });

            return edgeDeviceHealth;
        }

        private async Task<HealthStatusModel> GetCameraHealthStatus(Device device)
        {
            var health = new HealthStatusModel();

            // Check cache
            var cacheKey = $"{nameof(GetCameraHealthStatus)}-{device.DeviceId}";
            var cacheValue = (HealthStatusModel)_cache.Get(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            // Check online status
            if (!await CheckDeviceRecentlyOnline(device.DeviceId, _deviceRecentlyOnlineMaxMinutes, "camera"))
            {
                health.Code = HealthStatusCode.RED;
                health.Reason = "Camera offline";
                health.Action = "Contact support";
            }

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, health, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetCameraHealthStatus)} cache set");
            });

            return health;
        }

        public TenantOverview ConvertTimeToHealthStatus(TenantOverview overview)
        {
            foreach (var site in overview.Sites)
            {
                foreach (var edgeDevice in site.EdgeDevices)
                {
                    foreach (var leafDevice in edgeDevice.LeafDevices)
                    {
                        leafDevice.HealthStatus = new HealthStatusModel();
                        if (checkHealthTimeSpan(leafDevice.LastActiveTime))
                        {
                            leafDevice.HealthStatus.Code = HealthStatusCode.GREEN;
                        }
                        else
                        {
                            leafDevice.HealthStatus.Code = HealthStatusCode.RED;
                            leafDevice.HealthStatus.Reason = "Camera offline";
                            leafDevice.HealthStatus.Action = "Contact support";
                        }
                    }

                    edgeDevice.HealthStatus = new HealthStatusModel();
                    if (edgeDevice.LeafDevices.Count == 0)
                    {
                        edgeDevice.HealthStatus.Code = HealthStatusCode.AMBER;
                        edgeDevice.HealthStatus.Reason = "No camera found";
                        edgeDevice.HealthStatus.Action = "Configure in gateway menu";
                    }
                    else if (edgeDevice.LeafDevices.All(x => x.HealthStatus.Code == HealthStatusCode.AMBER))
                    {
                        edgeDevice.HealthStatus.Code = HealthStatusCode.AMBER;
                        edgeDevice.HealthStatus.Reason = "Camera(s) unusual";
                        edgeDevice.HealthStatus.Action = "Check camera menu";
                    }
                    else if (edgeDevice.LeafDevices.Any(x => x.HealthStatus.Code == HealthStatusCode.RED))
                    {
                        edgeDevice.HealthStatus.Code = HealthStatusCode.RED;
                        edgeDevice.HealthStatus.Reason = "Camera(s) offline";
                        edgeDevice.HealthStatus.Action = "Contact support";
                    }
                    else
                    {
                        edgeDevice.HealthStatus.Code = HealthStatusCode.GREEN;
                    }
                }

                site.HealthStatus = new HealthStatusModel();
                if (site.EdgeDevices.Count == 0)
                {
                    site.HealthStatus.Code = HealthStatusCode.AMBER;
                    site.HealthStatus.Reason = "No gateway";
                    site.HealthStatus.Action = "Configure in site menu";
                }
                else if (site.EdgeDevices.All(x => x.HealthStatus.Code == HealthStatusCode.AMBER))
                {
                    site.HealthStatus.Code = HealthStatusCode.AMBER;
                    site.HealthStatus.Reason = "Gateway(s) unusual";
                    site.HealthStatus.Action = "Check gateway menu";
                }
                else if (site.EdgeDevices.Any(x => x.HealthStatus.Code == HealthStatusCode.RED))
                {
                    site.HealthStatus.Code = HealthStatusCode.RED;
                    site.HealthStatus.Reason = "Gateway(s) offline";
                    site.HealthStatus.Action = "Contact support";
                }
                else
                {
                    site.HealthStatus.Code = HealthStatusCode.GREEN;
                }
            }

            return overview;
        }

        private bool checkHealthTimeSpan(string lastActiveTime)
        {
            DateTime dateTimeFromDB;
            if (DateTime.TryParse(lastActiveTime, out dateTimeFromDB))
            {
                if (DateTime.UtcNow < dateTimeFromDB.ToUniversalTime().AddMinutes(_deviceRecentlyOnlineMaxMinutes))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> CheckDeviceRecentlyOnline(string deviceId, int maxMinutes, string type)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/healthStatus?deviceId={deviceId}");
                var returnedHealthDataStatus = await _httpClient.SendAsync<HealthDataStatus>(request, CancellationToken.None);
                if (returnedHealthDataStatus != null && returnedHealthDataStatus.EdgeStarttime != null)
                {
                    TimeSpan span = DateTime.UtcNow.Subtract((DateTime)returnedHealthDataStatus.EdgeStarttime);
                    if (span < TimeSpan.FromMinutes(maxMinutes))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("CheckDeviceRecentlyOnline: " + e.Message);
            }

            return false;
        }
    }
}