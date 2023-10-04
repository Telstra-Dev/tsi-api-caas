using System;

namespace Telstra.Core.Data.Models
{
    public class LeafDeviceLatestTelemetryStatus
    {
        public string LeafDeviceId { get; set; }
        public string EdgeDeviceId { get; set; }
        public long? Timestamp { get; set; }
    }
}
