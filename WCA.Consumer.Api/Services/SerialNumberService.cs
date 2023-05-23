using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using WCA.Consumer.Api.Services.Contracts;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace WCA.Consumer.Api.Services
{
    public class SerialNumberService : ISerialNumberService
    {
        private readonly AppSettings _appSettings;
        private readonly IRestClient _httpClient;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDeviceService _deviceService;
        private IMemoryCache _cache { get; }
        private DateTimeOffset _shortCacheTime = DateTimeOffset.Now.AddSeconds(60);
        public SerialNumberService(IRestClient httpClient,
                        AppSettings appSettings,
                        IMapper mapper,
                        ILogger<OrganisationService> logger,
                        IDeviceService deviceService,
                        IMemoryCache cache)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
            _deviceService = deviceService;
            _cache = cache;
        }

        public async Task<SerialNumberModel> GetSerialNumberByValue(string value)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.EdgeDevicesAppHttp.BaseUri}/search?filter={value}&maxResults=1");
                var foundSerialNumbers = await _httpClient.SendAsync<IList<string>>(request, CancellationToken.None);

                if (foundSerialNumbers != null && foundSerialNumbers.Count > 0 && foundSerialNumbers[0] == value)
                {
                    return new SerialNumberModel
                    {
                        Value = foundSerialNumbers[0]
                    };
                }
                return null;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetSerialNumberByValue failed: {e.Message}");
                throw new Exception($"GetSerialNumberByValue failed: {e.Message}"); ;
            }
        }

        public async Task<IList<SerialNumberModel>> GetSerialNumbersByFilter(string filter, bool inactiveOnly = false, uint? maxResults = null)
        {
            IList<SerialNumberModel> serialNumberModels = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.EdgeDevicesAppHttp.BaseUri}/search?filter={filter}&maxResults={maxResults}");
                var foundSerialNumbers = await _httpClient.SendAsync<IList<string>>(request, CancellationToken.None);

                if (foundSerialNumbers != null && foundSerialNumbers.Count > 0)
                {
                    if (inactiveOnly)
                    {
                        IList<Device> activeDevices = null;

                        // Check cache
                        var cacheKey = $"{nameof(GetSerialNumbersByFilter)}-{nameof(_deviceService.GetGatewayDevices)}";
                        var cacheValue = (IList<Device>) _cache.Get(cacheKey);
                        if (cacheValue != null)
                        {
                            activeDevices = cacheValue;
                        }
                        else
                        {
                            activeDevices = await _deviceService.GetGatewayDevices(null, null);

                            // Set cache
                            _ = Task.Run(() =>
                            {
                                _cache.Set<IList<Device>>(cacheKey, activeDevices, _shortCacheTime);
                                _logger.LogTrace($"{nameof(GetSerialNumbersByFilter)}-{nameof(_deviceService.GetGatewayDevices)} cache set");
                            });
                        }

                        foundSerialNumbers = foundSerialNumbers.Select(d => d)
                                             .Except(activeDevices.Select(d => d.DeviceId),
                                             StringComparer.OrdinalIgnoreCase).ToList();
                    }

                    serialNumberModels = _mapper.Map<IList<SerialNumberModel>>(foundSerialNumbers);
                }
                return serialNumberModels;
            }
            catch (Exception e)
            {
                _logger.LogError($"GetSerialNumbers failed: {e.Message}");
                throw new Exception($"GetSerialNumbers failed: {e.Message}"); ;
            }
        }

    }
}