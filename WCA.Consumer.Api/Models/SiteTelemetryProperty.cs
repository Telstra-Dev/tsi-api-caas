using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class SiteTelemetryProperty
    {
        public IList<string> Objects { get; set; } = new List<string>();
        public IList<Location> Locations { get; set; } = new List<Location>();
    }

    public class TelemetryLocation
    {
        public int TagId { get; set; }
        public string TagType { get; set; }
        public string TagName { get; set; }
        public string TagValue { get; set; }
        public List<string> Objects { get; set; }
    }
}
