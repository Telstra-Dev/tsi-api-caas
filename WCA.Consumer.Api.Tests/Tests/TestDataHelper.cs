using System;
using System.Collections.Generic;
using Telstra.Common;
using Telstra.Core.Data.Entities;
using WCA.Consumer.Api.Models;
using AutoMapper;
using Moq;
using System.Runtime.ExceptionServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WCA.Customer.Api.Tests
{
    public class TestDataHelper
    {
        public static IList<Organisation> CreateOrganisations(int count)
        {
            var list = new List<Organisation>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new Organisation
                {
                    Id = $"customer-id-{i}",
                    CustomerId = $"customer-id-{i}",
                });
            }

            return list;
        }

        public static Organisation CreateOrganisation()
        {
            return CreateOrganisations(1)[0];
        }

        public static IList<OrganisationModel> CreateOrganisationModels(int count)
        {
            var list = new List<OrganisationModel>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new OrganisationModel
                {
                    Id = $"customer-id-{i}",
                    CustomerId = $"customer-id-{i}",
                });
            }

            return list;
        }

        public static OrganisationModel CreateOrganisationModel()
        {
            return CreateOrganisationModels(1)[0];
        }

        public static TenantOverview CreateTenantOverview(int siteCount, int gatewayCount, int cameraCount)
        {
            var tenantOverview = new TenantOverview()
            {
                TenantName = "Manual Test Organisation",
                Description = "",
                Sites = new List<SiteOverview>(),
            };

            for (int i = 0; i < siteCount; i += 1)
            {
                var siteOverview = new SiteOverview()
                {
                    SiteId = "site-test1",
                    SiteFriendlyName = "Test Site 1",
                    HealthStatus = new HealthStatusModel()
                    {
                        Code = HealthStatusCode.GREEN,
                        Reason = "",
                        Action = ""
                    },
                    EdgeDevices = new List<EdgeDeviceOverview>(),
                };

                for (int j = 0; j < gatewayCount; j += 1)
                {
                    var edgeDeviceOverview = new EdgeDeviceOverview()
                    {
                        EdgeDeviceId = "edge-device-test1",
                        EdgeDeviceFriendlyName = "Test Edge Device (Gateway) 1",
                        LastActiveTime = "Not found",
                        HealthStatus = new HealthStatusModel()
                        {
                            Code = HealthStatusCode.GREEN,
                            Reason = "",
                            Action = ""
                        },
                        LeafDevices = new List<LeafDeviceOverview>(),
                    };

                    for (int k = 0; k < gatewayCount; k += 1)
                    {
                        var leafDeviceOverview = new LeafDeviceOverview()
                        {
                            LeafId                  = "leaf-device-test1",
                            LeafFriendlyName        = "Test Leaf Device (Camera) 1",
                            LastActiveTime          = "Not found",
                            LastTelemetryTime       = null,
                            RequiresConfiguration   = null,
                            HealthStatus = new HealthStatusModel()
                            {
                                Code = HealthStatusCode.RED,
                                Reason = "",
                                Action = ""
                            }
                        };

                        edgeDeviceOverview.LeafDevices.Add(leafDeviceOverview);
                    }

                    siteOverview.EdgeDevices.Add(edgeDeviceOverview);
                }

                tenantOverview.Sites.Add(siteOverview);
            }

            return tenantOverview;
        }

        public static IList<Site> CreateSites(int count)
        {
            var list = new List<Site>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new Site
                {
                    SiteId = $"site-id-{i}",
                    Name = $"site-name-{i}",
                    CustomerId = "customer-id",

                    Active = true,
                    SiteLocationId = $"site-location-id-{i}",
                    Location = new SiteLocation
                    {
                        Id = $"site-location-id-{i}",
                    },
                    StoreCode = "StoreCode",
                    State = "State",
                    Type = "Type",
                    StoreFormat = "StoreFormat",
                    GeoClassification = "GeoClassification",
                    OrganisationId = "customer-id",
                    Region = "Region",
                    Tags = new List<SiteTag>
                        {
                            new SiteTag() { TagName = "Tag1Name", TagValue = "tag-1-value-a", },
                            new SiteTag() { TagName = "Tag1Name", TagValue = "tag-1-value-b", },
                            new SiteTag() { TagName = "Tag2Name", TagValue = "tag-2-value-a", },
                            new SiteTag() { TagName = "Tag2Name", TagValue = "tag-2-value-b", },
                        },
                });
            }

            return list;
        }

        public static Site CreateSite()
        {
            return CreateSites(1)[0];
        }

        public static IList<SiteModel> CreateSiteModels(int count)
        {
            var list = new List<SiteModel>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new SiteModel
                {
                    SiteId = i,
                    Name = $"site-name-{i}",
                    Metadata = new SiteMetadata
                    {
                        Tags = new List<int> { 1, 2, 3 }
                    },
                    Location = new SiteLocationModel
                    {
                        Id = i,
                        Address = new SiteAddress { Name = "242 Exhibition Street Melbourne" },
                        GeoLocation = new GeoLocation
                        {
                            Latitude = -37.809864,
                            Longitude = 144.969813,
                        }
                    },
                });
            }

            return list;
        }

        public static SiteModel CreateSiteModel()
        {
            return CreateSiteModels(1)[0];
        }

        public static IList<Device> CreateDevices(int count, DeviceType type = DeviceType.gateway)
        {
            var list = new List<Device>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new Device
                {
                    DeviceId = type == DeviceType.gateway ? $"edge-device-id-{i}" : $"leaf-device-id-{i}",
                    Name = type == DeviceType.gateway ? $"edge-device-name-{i}" : $"leaf-device-name-{i}",
                    CustomerId = "customer-id",
                    Type = type.ToString(),
                    SiteId = "site-id",
                    EdgeDeviceId = $"edge-device-id-{i}",
                });
            }

            return list;
        }

        public static Device CreateDevice(DeviceType type = DeviceType.gateway)
        {
            return CreateDevices(1, type)[0];
        }

        public static IList<DeviceModel> CreateDeviceModels(int count, DeviceType type = DeviceType.gateway)
        {
            var list = new List<DeviceModel>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new DeviceModel
                {
                    DeviceId = type == DeviceType.gateway ? $"edge-device-id-{i}" : $"leaf-device-id-{i}",
                    Name = type == DeviceType.gateway ? $"edge-device-name-{i}" : $"leaf-device-name-{i}",
                    EdgeDevice = $"edge-device-id-{i}",
                    Type = type,
                    CustomerId = "customer-id",
                    SiteId = "site-id",
                    EdgeCapable = true,
                });
            }

            return list;
        }

        public static DeviceModel CreateDeviceModel(DeviceType type = DeviceType.gateway)
        {
            return CreateDeviceModels(1)[0];
        }

        public static IList<Gateway> CreateGatewayModels(int count)
        {
            var list = new List<Gateway>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new Gateway
                {
                    DeviceId = $"edge-device-id-{i}",
                    Name = $"edge-device-name-{i}",
                    EdgeDevice = $"edge-device-id-{i}",
                    Type = DeviceType.gateway,
                    CustomerId = "customer-id",
                    SiteId = "site-id",
                    EdgeCapable = true,
                    Metadata = new GatewayMetadata
                    {
                        Hub = "Hub",
                        Auth = new Auth
                        {
                            IotHubConnectionString = "IotHubConnectionString",
                            SymmetricKey = new SymmetricKey
                            {
                                PrimaryKey = "PrimaryKey",
                                SecondaryKey = "SecondaryKey",
                            }
                        }
                    }
                });
            }

            return list;
        }

        public static Gateway CreateGatewayModel()
        {
            return CreateGatewayModels(1)[0];
        }

        public static IList<Camera> CreateCameraModels(int count)
        {
            var list = new List<Camera>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new Camera
                {
                    DeviceId = $"leaf-device-id-{i}",
                    Name = $"leaf-device-name-{i}",
                    EdgeDevice = $"edge-device-id-{i}",
                    Type = DeviceType.camera,
                    CustomerId = "customer-id",
                    SiteId = "site-id",
                    EdgeCapable = true,
                    Metadata = new CameraMetadata
                    {
                        Hub = "Hub",
                        Url = "Url",
                        Username = "Username",
                        Password = "Password",
                        Auth = new Auth
                        {
                            IotHubConnectionString = "IotHubConnectionString",
                            SymmetricKey = new SymmetricKey
                            {
                                PrimaryKey = "PrimaryKey",
                                SecondaryKey = "SecondaryKey",
                            }
                        }
                    }
                });
            }

            return list;
        }

        public static Camera CreateCameraModel()
        {
            return CreateCameraModels(1)[0];
        }

        public static IList<WCA.Consumer.Api.Models.Customer> CreateCustomers(int count)
        {
            var list = new List<WCA.Consumer.Api.Models.Customer>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new WCA.Consumer.Api.Models.Customer
                {
                    CustomerId = i,
                    Name = $"customer-name-{i}",
                    Alias = $"customer-alias-{i}",
                });
            }

            return list;
        }

        public static WCA.Consumer.Api.Models.Customer CreateCustomer()
        {
            return CreateCustomers(1)[0];
        }

        public static AppSettings CreateAppSettings()
        {
            return new AppSettings()
            {
                StorageAppHttp = new StorageAppSettings()
                {
                    BaseUri = "http://localhost:7000",
                },
                EdgeDevicesAppHttp = new StorageAppSettings()
                {
                    BaseUri = "http://localhost:7001",
                },
            };
        }

        public static Mock<IMapper> CreateMockMapper()
        {
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(a => a.Map<Gateway>(It.IsAny<Device>()))
                      .Returns((Device d) =>
                {
                    return new Gateway
                    {
                        DeviceId = d.DeviceId,
                        EdgeDevice = d.EdgeDeviceId,
                        SiteId = d.SiteId,
                        Name = d.Name,
                        Type = DeviceType.gateway,
                    };
                });
            mapperMock.Setup(a => a.Map<Camera>(It.IsAny<Device>()))
                      .Returns((Device d) =>
                {
                    return new Camera
                    {
                        DeviceId = d.DeviceId,
                        EdgeDevice = d.EdgeDeviceId,
                        SiteId = d.SiteId,
                        Name = d.Name,
                        Type = DeviceType.camera,
                    };
                });
            mapperMock.Setup(a => a.Map<Device>(It.IsAny<Camera>()))
                      .Returns((Camera d) =>
                {
                    return new Device
                    {
                        DeviceId = d.DeviceId,
                        EdgeDeviceId = d.EdgeDevice,
                        SiteId = d.SiteId,
                        Name = d.Name,
                        Type = "camera",
                    };
                });
            mapperMock.Setup(a => a.Map<Device>(It.IsAny<Gateway>()))
                      .Returns((Gateway d) =>
                {
                    return new Device
                    {
                        DeviceId = d.DeviceId,
                        EdgeDeviceId = d.EdgeDevice,
                        SiteId = d.SiteId,
                        Name = d.Name,
                        Type = "gateway",
                    };
                });
            mapperMock.Setup(a => a.Map<Device>(It.IsAny<DeviceModel>()))
                      .Returns((DeviceModel d) =>
                {
                    return new Device
                    {
                        DeviceId = d.DeviceId,
                        EdgeDeviceId = d.EdgeDevice,
                        SiteId = d.SiteId,
                        Name = d.Name,
                        Type = d.Type.ToString(),
                    };
                });

            mapperMock.Setup(a => a.Map<IList<OrgSearchTreeNode>>(It.IsAny<IList<Organisation>>()))
                      .Returns((IList<Organisation> orgs) =>
                {
                    var list = new List<OrgSearchTreeNode>();
                    foreach (var e in orgs)
                    {
                        list.Add(
                            new OrgSearchTreeNode
                            {
                                Type = "organisation",
                                Id = e.Id,
                                Href = $"/organisations?customerId={e.CustomerId}",
                                Text = e.CustomerName,
                            });
                    }
                    return list;
                });
            mapperMock.Setup(a => a.Map<IList<OrgSearchTreeNode>>(It.IsAny<IList<Site>>()))
                  .Returns((IList<Site> sites) =>
            {
                var list = new List<OrgSearchTreeNode>();
                foreach (var e in sites)
                {
                    list.Add(
                        new OrgSearchTreeNode
                        {
                            Type = "site",
                            Id = e.SiteId,
                            ParentId = e.SiteId,
                            Href = $"/sites?customerId={e.CustomerId}",
                            Text = e.Name,
                        });
                }
                return list;
            });
            mapperMock.Setup(a => a.Map<IList<OrgSearchTreeNode>>(It.IsAny<IList<Device>>()))
                  .Returns((IList<Device> devices) =>
            {
                var list = new List<OrgSearchTreeNode>();
                foreach (var e in devices)
                {
                    list.Add(
                        new OrgSearchTreeNode
                        {
                            Type = "device",
                            Id = e.DeviceId,
                            ParentId = e.SiteId,
                            Href = $"/devices/{e.DeviceId}",
                            Text = e.Name,
                        });
                }
                return list;
            });
            mapperMock.Setup(a => a.Map<OrganisationModel>(It.IsAny<Organisation>()))
                      .Returns((Organisation o) =>
                {
                    return new OrganisationModel
                    {
                        Id = o.Id,
                        CustomerId = o.CustomerId,
                    };
                });

            mapperMock.Setup(a => a.Map<IList<SiteModel>>(It.IsAny<IList<Site>>()))
                      .Returns((IList<Site> sites) =>
                {
                    var list = new List<SiteModel>();
                    foreach (var e in sites)
                    {
                        list.Add(
                            new SiteModel
                            {
                                SiteId = 1,
                                Name = e.Name
                            });
                    }
                    return list;
                });
            mapperMock.Setup(a => a.Map<SiteModel>(It.IsAny<Site>()))
                      .Returns((Site s) =>
                {
                    return new SiteModel
                    {
                        SiteId = 1,
                        Name = s.Name,
                    };
                });
            mapperMock.Setup(a => a.Map<IList<SerialNumberModel>>(It.IsAny<IList<string>>()))
                      .Returns((IList<string> serialNumbers) =>
                {
                    var list = new List<SerialNumberModel>();
                    foreach (var e in serialNumbers)
                    {
                        list.Add(
                            new SerialNumberModel
                            {
                                Value = e,
                            }
                        );
                    }
                    return list;
                });

            return mapperMock;
        }

        public static IList<string> CreateSerialNumbers(int count)
        {
            var list = new List<string>();

            for (int i = 1; i <= count; i++)
            {
                list.Add($"device-id-{i}");
            }

            return list;
        }

        public static string CreateSerialNumber()
        {
            return CreateSerialNumbers(1)[0];
        }

        public static IList<SerialNumberModel> CreateSerialNumberModels(int count)
        {
            var list = new List<SerialNumberModel>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new SerialNumberModel
                {
                    Value = $"device-id-{i}",
                });
            }

            return list;
        }

        public static SerialNumberModel CreateSerialNumberModel()
        {
            return CreateSerialNumberModels(1)[0];
        }

        public static IList<HealthStatusModel> CreateHealthStatusModels(int count)
        {
            var list = new List<HealthStatusModel>();

            for (int i = 1; i <= count; i++)
            {
                list.Add(new HealthStatusModel
                {
                    Code = HealthStatusCode.GREEN,
                    Reason = "Reason",
                    Action = "Action"
                });
            }

            return list;
        }

        public static HealthStatusModel CreateHealthStatusModel()
        {
            return CreateHealthStatusModels(1)[0];
        }

        public static HealthDataStatus CreateHealthDataStatus(string edgeDeviceId, string leafDeviceId, bool online = true)
        {
            return online ?
                new HealthDataStatus
                {
                    SvId = 1,
                    EdgeEdgedeviceid = edgeDeviceId,
                    EdgeLeafdeviceid = leafDeviceId,
                    EdgeStarttime = DateTime.UtcNow,
                    EdgeEndtime = DateTime.UtcNow,
                } :
                new HealthDataStatus
                {
                    SvId = 1,
                    EdgeEdgedeviceid = edgeDeviceId,
                    EdgeLeafdeviceid = leafDeviceId,
                    EdgeStarttime = new DateTime(2000, 3, 29, 10, 0, 0),
                    EdgeEndtime = new DateTime(2000, 3, 29, 10, 0, 0),
                };
        }

        public static List<TagModel> CreateTags(int count) 
        {
            var tagsList = new List<TagModel>();

            for (int i = 1; i <= count; i++)
            {
                tagsList.Add(new TagModel 
                {
                    Id = i,
                    Name = $"Test Tag {i}",
                    Category = "crossing",
                    Type = "trip line"
                });
            }

            return tagsList;
        }

        public static List<CreateTagModel> GenerateCreateTagsPayload(int count)
        {
            var tagsList = new List<CreateTagModel>();

            for (int i = 1; i <= count; i++)
            {
                tagsList.Add(new CreateTagModel 
                {
                    Name = $"Test Tag {i}",
                    Category = "crossing",
                    Type = "trip line"
                });
            }

            return tagsList;
        }

        public static string GenerateJwtToken()
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var issuer = "b2clogin";
            var claims = new List<Claim>
            {
                new Claim("email", "someone@team.telstra.com")
            };
            var jwt = new JwtSecurityToken(issuer, null, claims, null, DateTime.UtcNow.AddMinutes(60), null);
            return jwtHandler.WriteToken(jwt);
        }

        public static RtspFeedModel GenerateRtspFeedResponse() 
        {
            return new RtspFeedModel
            {
                DeviceId = "rtsp-integration-test01",
                TimeStamp = DateTimeOffset.Now,
                Message = "Test message!",
                Result = new Result
                {
                    Status = 200,
                    Payload = new Payload
                    {
                        ImageResponse = new ImageResponse
                        {
                            Image = "base64-format-string"
                        }
                    }
                }
            };
        }

        public static List<EdgeDeviceModel> CreateEdgeDevices(int count)
        {
            var edgeDevices = new List<EdgeDeviceModel>();

            for (int i = 0; i < count; i++)
            {
                edgeDevices.Add(new EdgeDeviceModel
                {
                    Id = i,
                    Name = $"test-edge-device-{i}",
                    SiteId = i,
                    EdgeEdgedeviceid = $"edge-device-id-{i}"
                });
            }

            return edgeDevices;
        }

        public static List<LeafDeviceModel> CreateLeafDevices(int count)
        {
            var leafDevices = new List<LeafDeviceModel>();

            for (int i = 0; i < count; i++)
            {
                leafDevices.Add(new LeafDeviceModel
                {
                    Id = i,
                    Name = $"test-leaf-device-{i}",
                    EdgeLeafdeviceid = $"leaf-device-id-{i}"
                });
            }

            return leafDevices;
        }
    }
}