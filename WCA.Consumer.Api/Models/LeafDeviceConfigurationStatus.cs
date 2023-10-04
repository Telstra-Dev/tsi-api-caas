
namespace Telstra.Core.Data.Models
{
    public class LeafDeviceConfigurationStatus
    {
        public string LeafDeviceId { get; set; }
        public string EdgeDeviceId { get; set; }
        public bool RequiresConfiguration { get; set; }
    }
}
