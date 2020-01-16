namespace Celcat.Verto.DataStore.Common.Configuration.PipelineElements
{
    using System.Xml.Serialization;

    [XmlType("publicStaging")]
    public class PublicStaging
    {
        [XmlAttribute("singleThreaded")]
        public bool SingleThreaded { get; set; }
    }
}
