using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telstra.Core.Data.Entities
{
    public class Device
    {
        [Key]
        [Required]
        [Column("device_id")]
        public string DeviceId {get; set;}

        [Column("name")]
        public string Name { get; set; }

        [Column("customer_id")]
        public string CustomerId { get; set; }
        
        [Column("site_id")]
        public string SiteId { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("edge_device_id")]
        public string EdgeDeviceId { get; set; }

        [Column("edge_capable")]
        public bool IsEdgeCapable { get; set; }

        [Column("metadata_url")]
        public string MetadataUrl { get; set; }

        [Column("metadata_username")]
        public string MetadataUsername { get; set; }

        [Column("metadata_password")]
        public string MetadataPassword { get; set; }

        [Column("metadata_hub")]
        public string MetadataHub { get; set; }

        [Column("metadata_auth_conn_string")]
        public string MetadataAuthConnString { get; set; }

        [Column("metadata_auth_symmetric_key")]
        public string MetadataAuthSymmetricKey { get; set; }
    }
}