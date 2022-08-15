using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("site_tag")]
    public class SiteTag
    {
        [Key]
        [Required]
        [Column("site_tag_id")]
        public string SiteTagId {get; set;}

        [Required]
        [Column("site_id")]
        public string SiteId {get; set;}

        [Required]
        [Column("tag_name")]
        public string TagName { get; set; }

        [Required]
        [Column("value")]
        public string TagValue { get; set; }
        
        public Site Site { get; set; }
    }
}