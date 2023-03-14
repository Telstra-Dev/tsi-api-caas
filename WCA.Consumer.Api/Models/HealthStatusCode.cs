using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WCA.Consumer.Api.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HealthStatusCode
    {
        GREEN,
        AMBER,
        RED,
    }
}