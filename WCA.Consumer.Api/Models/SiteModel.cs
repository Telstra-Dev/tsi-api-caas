using System.Collections.Generic;

namespace WCA.Consumer.Api.Models
{
    public class SiteModel
    {
        public string SiteId { get; set; }
        public string Name { get; set; }
        public SiteMetadata Metadata { get; set; }
        public SiteLocationModel Location { get; set; }
    }
}