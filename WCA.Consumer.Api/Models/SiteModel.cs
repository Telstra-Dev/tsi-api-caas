namespace WCA.Consumer.Api.Models
{
    public class SiteModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public bool Active { get; set; } = true;
        public SiteMetadata Metadata { get; set; }
        public SiteLocationModel Location { get; set; }
        public string CreatedAt { get; set; }
    }
}