namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlType("source")]
    public class Source
    {
        [XmlArray("timetables", IsNullable = false)]
        [XmlArrayItem("timetable", IsNullable = false)]
        public List<SourceTimetableConfiguration> Timetables { get; set; }

        public Source()
        {
            Timetables = new List<SourceTimetableConfiguration>();
        }
    }
}
