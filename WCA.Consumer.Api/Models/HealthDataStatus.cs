using System;

namespace Telstra.Core.Data.Entities
{
    public partial class HealthDataStatus
    {
        public long SvId { get; set; }
        public long? SvJsonId { get; set; }
        public string EdgeHubhostname { get; set; }
        public string EdgeEdgedeviceid { get; set; }
        public string EdgeSpecversion { get; set; }
        public string EdgeCorrelationid { get; set; }
        public string EdgeLeafdeviceid { get; set; }
        public string EdgeModuleid { get; set; }
        public string EdgePipelinename { get; set; }
        public DateTime? EdgeStarttime { get; set; }
        public DateTime? EdgeEndtime { get; set; }
        public int? EdgeDatasize { get; set; }
        public string EdgeType { get; set; }
        public string LeafSchema { get; set; }
        public DateTime? LeafTimestamp { get; set; }
        public string LeafInputuri { get; set; }
        public int? LeafWidth { get; set; }
        public int? LeafHeight { get; set; }
        public string LeafName { get; set; }
        public decimal? LeafConfidencethreshold { get; set; }
        public decimal? LeafAveragefps { get; set; }
        public decimal? LeafTracktime { get; set; }
        public decimal? LeafPreprocesstime { get; set; }
        public decimal? LeafDetectiontime { get; set; }
        public decimal? LeafFeatureextractiontime { get; set; }
        public decimal? LeafAssociationtime { get; set; }
        public decimal? LeafGeometrytime { get; set; }
        public DateTime SvInsertAt { get; set; }
    }
}
