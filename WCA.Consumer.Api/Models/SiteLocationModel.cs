namespace WCA.Consumer.Api.Models
{
    public class SiteLocationModel
    {
            public string Id { get; set; }
            public string Address { get; set; } 
            public GeoLocation GeoLocation {get; set;}
    }
}
