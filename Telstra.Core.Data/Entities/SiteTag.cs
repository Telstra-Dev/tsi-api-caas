using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("site_tag")]
    public class SiteTag
    {
        [Key]
        [Required]
        [Column("site_id")]
        public string SiteId { get; set; }

        public Site Site {get; set;}

        [Key]
        [Required]
        [Column("tag_id")]
        public int TagId { get; set; }

        public Tag Tag {get; set;}
   }
}