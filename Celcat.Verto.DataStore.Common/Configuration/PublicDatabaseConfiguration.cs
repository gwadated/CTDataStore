namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Xml.Serialization;

    [XmlType("publicDatabase")]
    public class PublicDatabaseConfiguration
    {
        [XmlElement("connectionString")]
        public string ConnectionString { get; set; }
    }
}
