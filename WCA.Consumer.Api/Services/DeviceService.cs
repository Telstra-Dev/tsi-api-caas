using System.Collections;
using Telstra.Core.Repo;
using Telstra.Core.Contracts;
using Telstra.Core.Data.Entities;

namespace WCA.Consumer.Api.Services
{
    public class DeviceService : IDeviceService
    {
        public DeviceService()
        {
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
                IoTHubConnectionString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo="
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
            devices.Add(gateway);

            CameraMetadata cameraMetadata = new CameraMetadata {
                Url = "hello.com",
                Username = "fred120",
                Password = "sdfsdfsd"
            };
            Camera camera = new Camera {
                DeviceId = "0448659b-eb21-410b-809c-c3b4879c9b48",
                Name = "Blue Mile Northern Entrance",
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

        public DeviceBase GetDevice(string deviceId)
        {
            SymmetricKey symmetricKey = new SymmetricKey {
                PrimaryKey = "Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo=",
                SecondaryKey = "8zbjhgT0VvVGMgGiAe9D9oyt+ECACvCGtYxvXlJIOY8="
            };
            Auth auth = new Auth {
                SymmetricKey = symmetricKey,
                IoTHubConnectionString = "HostName=tcp-azu0032-ae-iot-sv01-dev.azure-devices.net;DeviceId=3917acd9-2185-48a0-a71a-905316e2aae2;SharedAccessKey=Yi1J4AtlTMrIYYFHZHdQ6OBQUmALsbiMTliAxK/F1mo="
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

        public Camera CreateCameraDevice(Camera device)
        {
            return device;
        }

        public Gateway CreateEdgeDevice(Gateway device)
        {
            return device;
        }
    }
}