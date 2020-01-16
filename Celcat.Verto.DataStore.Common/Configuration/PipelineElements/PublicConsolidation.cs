namespace Celcat.Verto.DataStore.Common.Configuration.PipelineElements
{
    using System.Xml.Serialization;

    [XmlType("publicConsolidation")]
    public class PublicConsolidation
    {
        [XmlAttribute("singleThreaded")]
        public bool SingleThreaded { get; set; }
    }
}
