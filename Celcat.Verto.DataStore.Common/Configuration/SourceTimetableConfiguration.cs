namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Xml.Serialization;

    [XmlType("timetable")]
    public class SourceTimetableConfiguration
    {
        [XmlElement("connectionString", IsNullable = false)]
        public string ConnectionString { get; set; }
    }
}
