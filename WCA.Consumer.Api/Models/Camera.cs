using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WCA.Consumer.Api.Models
{
    public class Camera : DeviceModel
    {
        public CameraMetadata Metadata { get; set; } 
    }
}
