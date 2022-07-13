namespace Telstra.Core.Data.Entities
{
    public class DeviceBase
    {
            public string DeviceId { get; set; }
            public string Name { get; set; } 
            public string EdgeDevice {get; set;}
            public string CustomerId {get;set;}
            public string SiteId {get; set;}
            public string Type {get; set;}
            public bool EdgeCapable {get; set;}
            public long CreatedAt {get; set;}
            public bool Active {get; set;}
    }
}
