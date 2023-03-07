using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WCA.Consumer.Api.Models
{
    public class SerialNumberModel
    {
        public int SerialNumberId { get; set; }

        public string Value { get; set; }

        public string DeviceId { get; set; }
    }
}
