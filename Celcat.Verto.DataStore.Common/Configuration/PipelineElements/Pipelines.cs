namespace Celcat.Verto.DataStore.Common.Configuration.PipelineElements
{
    using System.Xml.Serialization;

    [XmlType("pipelines")]
    public class Pipelines
    {
        [XmlElement("adminStaging")]
        public AdminStaging AdminStaging { get; set; }

        [XmlElement("adminDiff")]
        public AdminDiff AdminDiff { get; set; }

        [XmlElement("adminHistory")]
        public AdminHistory AdminHistory { get; set; }

        [XmlElement("publicConsolidation")]
        public PublicConsolidation PublicConsolidation { get; set; }

        [XmlElement("publicStaging")]
        public PublicStaging PublicStaging { get; set; }

        [XmlElement("publicTempUpsert")]
        public PublicTempUpsert PublicTempUpsert { get; set; }

        [XmlElement("publicTransformation")]
        public PublicTransformation PublicTransformation { get; set; }
    }
}
