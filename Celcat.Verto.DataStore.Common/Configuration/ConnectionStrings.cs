namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("connectionStrings")]
    public class ConnectionStrings
    {
        [XmlElement("adminDatabase")]
        public string AdminDatabase { get; set; }

        [XmlElement("publicDatabase")]
        public string PublicDatabase { get; set; }

        [XmlArray("timetables")]
        [XmlArrayItem("timetable")]
        public List<string> Timetables { get; set; }

        public ConnectionStrings()
        {
            Timetables = new List<string>();
        }
    }
}
