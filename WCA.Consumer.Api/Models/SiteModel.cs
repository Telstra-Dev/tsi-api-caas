namespace WCA.Consumer.Api.Models
{
    public class SiteModel
    {
        public int SiteId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; } = true;
        public SiteMetadata Metadata { get; set; }
        public SiteLocationModel Location { get; set; }
    }
}