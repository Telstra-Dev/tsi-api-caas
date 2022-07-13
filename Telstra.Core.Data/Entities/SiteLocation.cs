namespace Telstra.Core.Data.Entities
{
    public class SiteLocation
    {
            public string SiteLocationId { get; set; }
            public string SiteAddress { get; set; } 
            public GeoLocation GeoLocation {get; set;}
    }
}
