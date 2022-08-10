using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("organisation")]
    public partial class Organisation
    {
        [Key]
        [Required]
        [Column("organisation_id")]
        public string Id {get;set;}

        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; } 

        [Column("parent")]
        public string Parent {get; set;}

        [Column("alias")]
        public string Alias {get; set;}

        [Column("created_at")]
        public long? CreatedAt {get; set;}
        
        public ICollection<Organisation> Children {get; set;}

        public ICollection<Site> Sites {get; set;}
    }
}

