namespace WCA.Consumer.Api.Models
{
    public class SiteLocationModel
    {
            public string Id { get; set; }
            public SiteAddress Address { get; set; } 
            public GeoLocation GeoLocation {get; set;}
    }
}
