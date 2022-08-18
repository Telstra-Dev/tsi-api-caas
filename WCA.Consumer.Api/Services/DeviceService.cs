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

namespace WCA.Consumer.Api.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public DeviceService(HttpClient httpClient,
                        AppSettings appSettings, 
                        IMapper mapper, 
                        ILogger<OrganisationService> logger)
        {
            this._httpClient = httpClient;
            this._appSettings = appSettings;
            this._mapper = mapper;
            this._logger = logger;
        }

        public async Task<ArrayList> GetDevices(string customerId, string siteId)
        {
            ArrayList devices = new ArrayList();
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var returnedDevices = JsonConvert.DeserializeObject<IList<Device>>(reply);
                    if (returnedDevices != null && returnedDevices.Count > 0)
                    {
                        foreach (var returnedDevice in returnedDevices)
                        {
                            if (returnedDevice.Type == DeviceType.gateway.ToString())
                                devices.Add(_mapper.Map<Gateway>(returnedDevice));
                            else // camera
                                devices.Add(_mapper.Map<Camera>(returnedDevice));
                        }
                    }
                }
                else
                {
                    _logger.LogError("GetDevices failed with error: " + reply);
                    throw new Exception($"Error getting devices. {response.StatusCode} Response code from downstream: " + reply); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetDevices: " + e.Message);
                throw new Exception(e.Message);;
            }
            return devices;
        }

        public async Task<DeviceModel> GetDevice(string deviceId)
        {
            DeviceModel foundMappedDevice = null;
            try
            {
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices/{deviceId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    var foundDevice = JsonConvert.DeserializeObject<Device>(reply);
                    if (foundDevice != null)
                    {
                        if (foundDevice.Type == DeviceType.gateway.ToString())
                            foundMappedDevice = _mapper.Map<Gateway>(foundDevice);
                        else // camera
                            foundMappedDevice = _mapper.Map<Camera>(foundDevice);
                    }
                }
                else
                {
                    _logger.LogError("GetDevice failed with error: " + reply);
                    throw new Exception($"Error getting device. {response.StatusCode} Response code from downstream: " + reply); 
                }
            }
            catch (Exception e)
            {
                _logger.LogError("GetDevice: " + e.Message);
                throw new Exception(e.Message);;
            }
            return foundMappedDevice;
        }

        public async Task<DeviceModel> DeleteDevice(string customerId, string deviceId)
        {
            DeviceModel deletedMappedDevice = null;
            try
            {
                var response = await _httpClient.DeleteAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&deviceId={deviceId}");
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
            }
            catch (Exception e)
            {
                _logger.LogError("DeleteDevice: " + e.Message);
                throw new Exception(e.Message);;
            }
            return deletedMappedDevice;
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
            Camera returnedMappedDevice= null;
            Device mappedDevice = _mapper.Map<Device>(saveCamera);
            mappedDevice.IsActive = true;
            try
            {
                var payload =JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                if (!isUpdate)
                    response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices", httpContent);
                else
                    response = await _httpClient.PutAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices/{saveCamera.DeviceId}", httpContent);

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
                throw new Exception(e.Message);;
            }
            return returnedMappedDevice;
        }

        private async Task<Gateway> SaveEdgeDevice(Gateway saveGateway, bool isUpdate = false)
        {
            Gateway returnedMappedDevice= null;
            Device mappedDevice = _mapper.Map<Device>(saveGateway);

            // @TODO Hacks for now (hard-code)
            mappedDevice.IsActive = true;
            mappedDevice.MetadataAuthConnString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";
            mappedDevice.MetadataAuthSymmetricKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";
            
            try
            {
                var payload =JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                if (!isUpdate)
                    response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices", httpContent);
                else
                    response = await _httpClient.PutAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices/{saveGateway.DeviceId}", httpContent);

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
                throw new Exception(e.Message);;
            }
            return returnedMappedDevice;
        }

    }
}