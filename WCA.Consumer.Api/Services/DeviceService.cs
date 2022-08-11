using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
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

        public ArrayList GetDevices(string customerId, string siteId)
        {
            ArrayList devices = new ArrayList();
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
                Name = "tva-sv-chad1",
                CustomerId = "manual-test-customer-id",
                SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
                EdgeDevice = "3917acd9-2185-48a0-a71a-905316e2aae2",
                EdgeCapable = true,
                Metadata = gatewayMetadata,
                CreatedAt = 1655347987378,
                Active = true
            };
            devices.Add(gateway);

            CameraMetadata cameraMetadata = new CameraMetadata {
                Url = "hello.com",
                Username = "fred120",
                Password = "sdfsdfsd"
            };
            Camera camera = new Camera {
                DeviceId = "0448659b-eb21-410b-809c-c3b4879c9b48",
                Name = "tva-sv-chad1-camera1",
                CustomerId = "manual-test-customer-id",
                SiteId = "bceead95-5b9d-47bc-9d93-4740db6c1292",
                EdgeDevice = "3917acd9-2185-48a0-a71a-905316e2aae2",
                EdgeCapable = false,
                Metadata = cameraMetadata,
                CreatedAt = 1655348052855,
                Active = true
            };
            devices.Add(camera);

            return devices;
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