using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WCA.Consumer.Api.Models
{
    public class DeviceModel
    {
        public string DeviceId { get; set; }
        
        public string Name { get; set; } 

        public string EdgeDevice {get; set;}

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceType Type {get; set;}

        public string CustomerId {get;set;}

        public string SiteId {get; set;}

        public bool EdgeCapable {get; set;}

        public long CreatedAt {get; set;}

        public bool Active {get; set;}

        public string ModuleName {get; set;}
    }
}
