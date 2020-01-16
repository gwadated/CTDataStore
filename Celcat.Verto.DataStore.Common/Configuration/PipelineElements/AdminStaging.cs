namespace Celcat.Verto.DataStore.Common.Configuration.PipelineElements
{
    using System.Xml.Serialization;

    [XmlType("adminStaging")]
    public class AdminStaging
    {
        [XmlAttribute("singleThreaded")]
        public bool SingleThreaded { get; set; }
    }
}
