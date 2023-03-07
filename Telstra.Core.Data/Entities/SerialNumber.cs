using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    [Table("serial_number")]
    public class SerialNumber
    {
        [Key]
        [Required]
        [Column("serial_number_id")]
        public int SerialNumberId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("device_id")]
        public string DeviceId { get; set; }
    }
}