using System;

namespace WCA.Consumer.Api.Models
{
    public class LeafDeviceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int EdgeDeviceId { get; set; }
        public int LeafDeviceConfId { get; set; }
        public int InstallVendorId { get; set; }
        public int SupportVendorId { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string EdgeLeafdeviceid { get; set; }
        public string SvAlias { get; set; }
        public string SvCode { get; set; }
        public string SvCaption { get; set; }
        public string SvLabel { get; set; }
        public string SvFlag { get; set; }
        public string SvStatus { get; set; }
        public string SvFeature { get; set; }
        public string SvNote { get; set; }
        public DateTime InsertAt { get; set; }
        public string InsertBy { get; set; }
        public string InsertHost { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public string UpdateHost { get; set; }
        public DateTime? DeleteAt { get; set; }
        public string DeleteBy { get; set; }
        public string DeleteHost { get; set; }
    }
}
