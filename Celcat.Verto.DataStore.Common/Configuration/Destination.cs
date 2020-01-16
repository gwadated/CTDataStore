namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Xml.Serialization;

    [XmlType("destination")]
    public class Destination
    {
        [XmlElement("adminDatabase", IsNullable = false)]
        public AdminDatabaseConfiguration AdminDatabase { get; set; }

        [XmlElement("publicDatabase", IsNullable = false)]
        public PublicDatabaseConfiguration PublicDatabase { get; set; }

        public Destination()
        {
            AdminDatabase = new AdminDatabaseConfiguration();
            PublicDatabase = new PublicDatabaseConfiguration();
        }
    }
}
