namespace Telstra.Core.Data.Entities
{
    public class SiteLocation
    {
            public string Id { get; set; }
            public string Address { get; set; } 
            public GeoLocation GeoLocation {get; set;}
    }
}
