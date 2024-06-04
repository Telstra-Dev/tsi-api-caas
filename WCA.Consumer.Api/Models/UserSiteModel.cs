namespace WCA.Consumer.Api.Models
{
    public class UserSiteModel
    {
        public int SiteId { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string TenantName { get; set; }
        public string LocationId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
