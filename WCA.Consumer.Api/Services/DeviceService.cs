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
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var response = await _httpClient.GetAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices?customerId={customerId}&siteId={siteId}");
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedDevices = JsonConvert.DeserializeObject<IList<Device>>(reply);
                    foreach (var returnedDevice in returnedDevices)
                    {
                        if (returnedDevice.Type == DeviceType.gateway.ToString())
                            devices.Add(_mapper.Map<Gateway>(returnedDevice));
                        else // camera
                            devices.Add(_mapper.Map<Camera>(returnedDevice));
                    }
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
            return devices;
            // ArrayList devices = new ArrayList();
            // SymmetricKey symmetricKey = new SymmetricKey {
            //     PrimaryKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=",
            //     SecondaryKey = "8zbjhgT0VvVGMgGiAe9D9oyt+ECACvCGtYxvXlJIOY8="
            // };
            // Auth auth = new Auth {
            //     SymmetricKey = symmetricKey,
            //     IotHubConnectionString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo="
            // };
            // GatewayMetadata gatewayMetadata = new GatewayMetadata {
            //     Hub = "tcp-azu0032-ae-iot-sv01-dev.azure-devices.net",
            //     Auth = auth
            // };
            // Gateway gateway = new Gateway {
            //     DeviceId = "3917acd9-2185-48a0-a71a-905316e2aae2",
            //     Name = "tva-sv-chad1",
            //     CustomerId = "manual-test-customer-id",
            //     SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
            //     EdgeDevice = "3917acd9-2185-48a0-a71a-905316e2aae2",
            //     EdgeCapable = true,
            //     Metadata = gatewayMetadata,
            //     CreatedAt = 1655347987378,
            //     Active = true
            // };
            // devices.Add(gateway);

            // CameraMetadata cameraMetadata = new CameraMetadata {
            //     Url = "hello.com",
            //     Username = "fred120",
            //     Password = "sdfsdfsd"
            // };
            // Camera camera = new Camera {
            //     DeviceId = "0448659b-eb21-410b-809c-c3b4879c9b48",
            //     Name = "tva-sv-chad1-camera1",
            //     CustomerId = "manual-test-customer-id",
            //     SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
            //     EdgeDevice = "3917acd9-2185-48a0-a71a-905316e2aae2",
            //     EdgeCapable = false,
            //     Metadata = cameraMetadata,
            //     CreatedAt = 1655348052855,
            //     Active = true
            // };
            // devices.Add(camera);

            // return devices;
        }

        public DeviceModel GetDevice(string deviceId)
        {
            SymmetricKey symmetricKey = new SymmetricKey {
                PrimaryKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=",
                SecondaryKey = "8zbjhgT0VvVGMgGiAe9D9oyt+ECACvCGtYxvXlJIOY8="
            };
            Auth auth = new Auth {
                SymmetricKey = symmetricKey,
                IotHubConnectionString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo="
            };
            GatewayMetadata gatewayMetadata = new GatewayMetadata {
                Hub = "tcp-azu0032-ae-iot-sv01-dev.azure-devices.net",
                Auth = auth
            };
            Gateway gateway = new Gateway {
                DeviceId = "3917acd9-2185-48a0-a71a-905316e2aae2",
                Name = "Blue Mile Northern Gateway",
                CustomerId = "manual-test-customer-id",
                SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
                EdgeDevice = "3917acd9-2185-48a0-a71a-905316e2aae2",
                EdgeCapable = true,
                Metadata = gatewayMetadata,
                CreatedAt = 1655347987378,
                Active = true
            };
            return gateway;
        }

        public async Task<DeviceModel> DeleteDevice(string customerId, string deviceId)
        {
            DeviceModel deletedMappedDevice = null;
            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
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

        public Camera UpdateCameraDevice(string id, Camera camera)
        {
            return camera;
        }

        public Gateway UpdateEdgeDevice(string id, Gateway gateway)
        {
            return gateway;
        }

        public async Task<Camera> CreateCameraDevice(Camera newCamera)
        {
            Camera returnedMappedDevice= null;
            Device mappedDevice = _mapper.Map<Device>(newCamera);
            mappedDevice.IsActive = true;
            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var payload =JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices", httpContent);
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

        public async Task<Gateway> CreateEdgeDevice(Gateway newGateway)
        {
            Gateway returnedMappedDevice= null;
            Device mappedDevice = _mapper.Map<Device>(newGateway);
            // @TODO Hacks for now (hard-code)
            mappedDevice.IsActive = true;
            mappedDevice.MetadataAuthConnString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";
            mappedDevice.MetadataAuthSymmetricKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=";
            try
            {
                _logger.LogTrace("Storage app base uri:" + _appSettings.StorageAppHttp.BaseUri);
                var payload =JsonConvert.SerializeObject(mappedDevice);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_appSettings.StorageAppHttp.BaseUri}/devices", httpContent);
                var reply = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnedDevice = JsonConvert.DeserializeObject<Device>(reply);
                    returnedMappedDevice = _mapper.Map<Gateway>(returnedDevice);
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
    }
}