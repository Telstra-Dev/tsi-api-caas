using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
using System.Threading;

namespace WCA.Consumer.Api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IRestClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public DeviceService(IRestClient httpClient,
                        AppSettings appSettings,
                        IMapper mapper,
                        ILogger<OrganisationService> logger)
        {
            _httpClient = httpClient;
            _appSettings = appSettings;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ArrayList> GetDevices(string customerId, string siteId)
        {
            ArrayList devices = new ArrayList();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}");
                var returnedDevices = await _httpClient.SendAsync<IList<Device>>(request, CancellationToken.None);

                if (returnedDevices != null)
                {
                    foreach (var returnedDevice in returnedDevices)
                    {
                        if (returnedDevice.Type == DeviceType.gateway.ToString())
                            devices.Add(_mapper.Map<Gateway>(returnedDevice));
                        else // camera
                            devices.Add(_mapper.Map<Camera>(returnedDevice));
                    }
                }
                return devices;
            }
            catch (Exception e)
            {
                _logger.LogError("GetDevices: " + e.Message);
                throw new Exception(e.Message); ;
            }
        }

        public async Task<IList<Device>> GetGatewayDevices(string customerId, string siteId)
        {
            var devices = new List<Device>();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}");
                var returnedDevices = await _httpClient.SendAsync<IList<Device>>(request, CancellationToken.None);

                if (returnedDevices != null)
                {
                    foreach (var returnedDevice in returnedDevices)
                    {
                        if (returnedDevice.Type == DeviceType.gateway.ToString())
                        {
                            devices.Add(returnedDevice);
                        }
                    }
                }
                return devices;
            }
            catch (Exception e)
            {
                _logger.LogError("GetGatewayDevices: " + e.Message);
                throw new Exception(e.Message); ;
            }
        }

        public async Task<DeviceModel> GetDevice(string deviceId, string customerId)
        {
            DeviceModel foundMappedDevice = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{deviceId}?customerId={customerId}");
                var returnedDevice = await _httpClient.SendAsync<Device>(request, CancellationToken.None);

                if (returnedDevice != null)
                {
                    if (returnedDevice.Type == DeviceType.gateway.ToString())
                        foundMappedDevice = _mapper.Map<Gateway>(returnedDevice);
                    else // camera
                        foundMappedDevice = _mapper.Map<Camera>(returnedDevice);
                }

                return foundMappedDevice;

            }
            catch (Exception e)
            {
                _logger.LogError("GetDevice: " + e.Message);
                throw new Exception(e.Message); ;
            }
        }

        public async Task<DeviceModel> DeleteDevice(string customerId, string deviceId)
        {
            DeviceModel deletedMappedDevice = null;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&deviceId={deviceId}");
                var response = await _httpClient.SendAsync(request, CancellationToken.None);

                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var deletedDevice = JsonConvert.DeserializeObject<Device>(reply);
                    if (deletedDevice != null)
                    {
                        if (deletedDevice.Type == DeviceType.gateway.ToString())
                            deletedMappedDevice = _mapper.Map<Gateway>(deletedDevice);
                        else // camera
                            deletedMappedDevice = _mapper.Map<Camera>(deletedDevice);
                    }
                }
                else
                {
                    _logger.LogError("DeleteDevice failed with error: " + reply);
                    throw new Exception($"Error deleting a device. {response.StatusCode} Response code from downstream: " + reply);
                }
                return deletedMappedDevice;
            }
            catch (Exception e)
            {
                _logger.LogError("DeleteDevice: " + e.Message);
                throw new Exception(e.Message); ;
            }
        }

        public async Task<Camera> CreateCameraDevice(Camera newCamera)
        {
            return await SaveCameraDevice(newCamera);
        }

        public async Task<Camera> UpdateCameraDevice(string id, Camera updateCamera)
        {
            return await SaveCameraDevice(updateCamera, true);
        }

        public async Task<Gateway> CreateEdgeDevice(Gateway newGateway)
        {
            return await SaveEdgeDevice(newGateway);
        }

        public async Task<Gateway> UpdateEdgeDevice(string id, Gateway gateway)
        {
            return await SaveEdgeDevice(gateway, true);
        }

        private async Task<Camera> SaveCameraDevice(Camera saveCamera, bool isUpdate = false)
        {
            Camera returnedMappedDevice = null;
            Device mappedDevice = _mapper.Map<Device>(saveCamera);
            mappedDevice.IsActive = true;
            try
            {
                var payload = JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                HttpRequestMessage request;

                if (!isUpdate)
                {
                    request = new HttpRequestMessage(HttpMethod.Post, $"{_appSettings.StorageAppHttp.BaseUri}/devices");
                }
                else
                {
                    request = new HttpRequestMessage(HttpMethod.Put, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{saveCamera.DeviceId}");
                }
                request.Content = httpContent;
                response = await _httpClient.SendAsync(request, CancellationToken.None);

                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedDevice = JsonConvert.DeserializeObject<Device>(reply);
                    returnedMappedDevice = _mapper.Map<Camera>(returnedDevice);
                }
                else
                {
                    _logger.LogError("CreateEdgeDevice failed with error: " + reply);
                    throw new Exception($"Error creating an edge device. {response.StatusCode} Response code from downstream: " + reply);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("CreateEdgeDevice: " + e.Message);
                throw new Exception(e.Message); ;
            }
            return returnedMappedDevice;
        }

        private async Task<Gateway> SaveEdgeDevice(Gateway saveGateway, bool isUpdate = false)
        {
            Gateway returnedMappedDevice = null;
            Device mappedDevice = _mapper.Map<Device>(saveGateway);

            // @TODO Hacks for now (hard-code)
            mappedDevice.IsActive = true;
            mappedDevice.MetadataAuthConnString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";
            mappedDevice.MetadataAuthSymmetricKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";

            try
            {
                var payload = JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                HttpRequestMessage request;

                if (!isUpdate)
                    request = new HttpRequestMessage(HttpMethod.Post, $"{_appSettings.StorageAppHttp.BaseUri}/devices");
                else
                    request = new HttpRequestMessage(HttpMethod.Put, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{saveGateway.DeviceId}");

                request.Content = httpContent;
                response = await _httpClient.SendAsync(request, CancellationToken.None);

                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedDevice = JsonConvert.DeserializeObject<Device>(reply);
                    returnedMappedDevice = _mapper.Map<Gateway>(returnedDevice);
                }
                else
                {
                    _logger.LogError("SaveEdgeDevice failed with error: " + reply);
                    throw new Exception($"Error saving an edge device. {response.StatusCode} Response code from downstream: " + reply);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("SaveEdgeDevice: " + e.Message);
                throw new Exception(e.Message); ;
            }
            return returnedMappedDevice;
        }

        public async Task<IList<Device>> GetLeafDevicesForGateway(string deviceId)
        {
            var devices = new List<Device>();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_appSettings.StorageAppHttp.BaseUri}/devices/{deviceId}/leafDevices");
                var response = await _httpClient.SendAsync(request, CancellationToken.None);
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var returnedDevices = JsonConvert.DeserializeObject<IList<Device>>(reply);
                    if (returnedDevices != null && returnedDevices.Count > 0)
                    {
                        foreach (var returnedDevice in returnedDevices)
                        {
                            devices.Add(returnedDevice);
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return devices;
                }
                else
                {
                    _logger.LogError("GetLeafDevicesForGateway failed with error: " + reply);
                    throw new Exception($"Error getting leaf devices. {response.StatusCode} Response code from downstream: " + reply);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetLeafDevicesForGateway: " + e.Message);
                throw new Exception(e.Message); ;
            }
            return devices;
        }

    }
}