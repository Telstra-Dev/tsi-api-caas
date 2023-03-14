using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
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
using WCA.Consumer.Api.Controllers;
using WCA.Consumer.Api.Services.Contracts;

namespace WCA.Consumer.Api.Services
{
    public class HealthStatusService : IHealthStatusService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDeviceService _deviceService;
        private readonly ISiteService _siteService;
        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime = DateTimeOffset.Now.AddSeconds(60);

        public HealthStatusService(HttpClient httpClient,
                        AppSettings appSettings, 
                        IMapper mapper, 
                        ILogger<OrganisationService> logger,
                        IDeviceService deviceService,
                        ISiteService siteService,
                        IMemoryCache cache)
        {
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
            this._deviceService = deviceService;
            this._siteService = siteService;
            this._cache = cache;
        }

        public async Task<HealthStatusModel> GetDeviceHealthStatus(string deviceId)
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
                return await GetGatewayHealthStatus(device);
            }
            else
            {
                return await GetCameraHealthStatus(device);
            }
        }

        private async Task<HealthStatusModel> GetGatewayHealthStatus(Device device)
        {
            var health = new HealthStatusModel();

            // Check cache
            var cacheKey = $"{nameof(GetGatewayHealthStatus)}-{device.DeviceId}";
            var cacheValue = (HealthStatusModel) _cache.Get(cacheKey);
            if (cacheValue != null)
            {
                return cacheValue;
            }

            // TODO: Check if gateway offline

            var leafDevices = await _deviceService.GetLeafDevicesForGateway(device.DeviceId);

            if (leafDevices == null || leafDevices.Count == 0)
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "No cameras attached";
                health.Action = "Configure on gateway";
            }

            foreach (var leafDevice in leafDevices)
            {
                var deviceHealth = await GetCameraHealthStatus(leafDevice);
                if (deviceHealth.Code != HealthStatusCode.GREEN)
                {
                    health.Code = deviceHealth.Code;
                    health.Reason = deviceHealth.Reason;
                    health.Action = deviceHealth.Action;

                    break;
                }
            }

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, health, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetGatewayHealthStatus)} cache set");
            });

            return health;
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

            // TODO: Implement checks including camera offline, not capturing data correctly

            _ = Task.Run(() =>
            {
                _cache.Set<HealthStatusModel>(cacheKey, health, _shortCacheTime);
                _logger.LogTrace($"{nameof(GetCameraHealthStatus)} cache set");
            });

            return health;
        }

        public async Task<HealthStatusModel> GetSiteHealthStatus(string siteId)
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
            var health = new HealthStatusModel();

            var deviceModels = await _deviceService.GetDevices(null, site.SiteId);
            if (deviceModels == null || deviceModels.Count == 0)
            {
                health.Code = HealthStatusCode.AMBER;
                health.Reason = "No devices";
                health.Action = "Configure in site menu";
            }

            foreach (DeviceModel deviceModel in deviceModels)
            {
                if (deviceModel.Type == DeviceType.gateway)
                {
                    var device = _mapper.Map<Device>(deviceModel);
                    var deviceHealth = await GetGatewayHealthStatus(device);
                    if (deviceHealth.Code != HealthStatusCode.GREEN)
                    {
                        health.Code = deviceHealth.Code;
                        health.Reason = deviceHealth.Reason;
                        health.Action = deviceHealth.Action;

                        break;
                    }
                }
            }

            return health;
        }
    }
}