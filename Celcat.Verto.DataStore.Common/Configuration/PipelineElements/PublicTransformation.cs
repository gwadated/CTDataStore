namespace Celcat.Verto.DataStore.Common.Configuration.PipelineElements
{
    using System.Xml.Serialization;

    [XmlType("publicTransformation")]
    public class PublicTransformation
    {
        [XmlAttribute("singleThreaded")]
        public bool SingleThreaded { get; set; }
    }
}
