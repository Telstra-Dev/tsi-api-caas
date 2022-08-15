using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("site")]
    public class Site
    {
        [Key]
        [Required]
        [Column("site_id")]
        public string SiteId { get; set; }

        [Column("name")]
        public string Name { get; set; } 

        [Column("customer_id")]
        public string CustomerId {get; set;}

        [Column("is_active")]
        public bool Active {get; set;} = true;

        [Column("location_id")]
        public string SiteLocationId { get; set; }

        [Column("site_location")]
        public SiteLocation Location {get; set;}

        [Column("created_at")]
        public long CreatedAt {get; set;}

        [Column("store_code")]
        public string StoreCode { get; set; }

        [Column("state")]
        public string State { get; set; } 

        [Column("type")]
        public string Type {get; set;}

        [Column("store_format")]
        public string StoreFormat {get; set;}

        [Column("geo_classification")]
        public string GeoClassification {get; set;}

        [Column("region")]
        public string Region {get; set;}

        [Column("organisation_id")]
        public string OrganisationId {get; set;}

        public ICollection<SiteTag> Tags {get; set;}

        public Organisation Organisation {get; set;}
    }
}