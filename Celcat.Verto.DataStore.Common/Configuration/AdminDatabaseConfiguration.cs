namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Xml.Serialization;

    [XmlType("adminDatabase")]
    public class AdminDatabaseConfiguration
    {
        [XmlElement("connectionString")]
        public string ConnectionString { get; set; }
    }
}
