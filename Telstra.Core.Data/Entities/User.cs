using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    public partial class User
    {
        [Key]
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        public string Oid { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("ad_id")]
        public string ActiveDirectoryId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(20)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [MaxLength(20)]
        [Column("last_name")]
        public string LastName { get; set; }

        [MaxLength(20)]
        [Column("mobile")]
        public string Mobile { get; set; }

        public bool? CarParkingAllowed { get; set; }

        public int? FloorId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("alias")]
        public string Alias { get; set; }
    }
}
