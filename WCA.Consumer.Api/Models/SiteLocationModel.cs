namespace WCA.Consumer.Api.Models
{
    public class SiteLocationModel
    {
            // public int Id { get; set; }
            public SiteAddress Address { get; set; } 
            public GeoLocation GeoLocation {get; set;}
    }
}
