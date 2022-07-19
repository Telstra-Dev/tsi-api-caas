namespace Telstra.Core.Data.Entities
{
    public class Site
    {
            public string SiteId { get; set; }
            public string Name { get; set; } 
            public string CustomerId {get; set;}
            public bool Active {get; set;} = true;
            public SiteMetadata Metadata {get;set;}
            public SiteLocation Location {get; set;}
            public long CreatedAt {get; set;}
    }
}