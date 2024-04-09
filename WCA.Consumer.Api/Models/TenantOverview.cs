using System;
using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class TenantOverview
    {
        public string TenantName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IList<SiteOverview> Sites { get; set; } = new List<SiteOverview>();
    }

    public class SiteOverview
    {
        public int SiteId { get; set; }
        public string SiteFriendlyName { get; set; }
        public HealthStatusModel HealthStatus { get; set; }
        public IList<EdgeDeviceOverview> EdgeDevices { get; set; } = new List<EdgeDeviceOverview>();
    }

    public class EdgeDeviceOverview
    {
        public int EdgeDeviceId { get; set; }
        public string SerialNumber { get; set; }
        public string EdgeDeviceFriendlyName { get; set; }
        public string LastActiveTime { get; set; }
        public HealthStatusModel HealthStatus { get; set; }
        public IList<LeafDeviceOverview> LeafDevices { get; set; } = new List<LeafDeviceOverview>();
    }

    public class LeafDeviceOverview
    {
        public int LeafId { get; set; }
        public string PipelineDeviceId { get; set; }
        public string LeafFriendlyName { get; set; }
        public string LastActiveTime { get; set; }
        public long? LastTelemetryTime { get; set; }
        public bool? RequiresConfiguration { get; set; }
        public HealthStatusModel HealthStatus { get; set; }
    }
}