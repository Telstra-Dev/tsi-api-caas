using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("tag")]
    public class Tag
    {
        [Key]
        [Required]
        [Column("tag_id")]
        public int TagId {get; set;}

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public string Value { get; set; }
        
        public ICollection<SiteTag> SiteTags { get; set; } = new List<SiteTag>();
    }
}