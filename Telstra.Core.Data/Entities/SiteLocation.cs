using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("site_location")]
    public class SiteLocation
    {
        [Key]
        [Required]
        [Column("site_location_id")]
        public string Id { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("latitude")]
        public double Latitude { get; set; }

        [Column("longitude")]
        public double Longitude { get; set; } 
    }
}

