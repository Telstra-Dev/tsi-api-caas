using System.Collections.Generic;
namespace WCA.Consumer.Api.Models
{
    public class SiteMetadata
    {
            public string StoreCode { get; set; }
            public string State { get; set; } 
            public string Type {get; set;}
            public string StoreFormat {get; set;}
            public string GeoClassification {get; set;}
            public string Region {get; set;}
            public Dictionary<string, string[]> Tags {get; set;}
    }
}
