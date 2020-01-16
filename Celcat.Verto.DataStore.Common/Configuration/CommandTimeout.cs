namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System.Xml.Serialization;

    /// <summary>
    /// Specify command timeouts for source and destination servers
    /// NB - these are just the _command_ timeouts; the connection timouts are 
    /// specified in the relevant connection strings
    /// </summary>
    [XmlType("commandTimeouts")]
    public class CommandTimeout
    {
        [XmlAttribute("sourceTimetables")]
        public int SourceTimetables { get; set; }

        [XmlAttribute("adminDatabase")]
        public int AdminDatabase { get; set; }

        [XmlAttribute("publicDatabase")]
        public int PublicDatabase { get; set; }

        public CommandTimeout()
        {
            SourceTimetables = 60;
            AdminDatabase = 180;
            PublicDatabase = 180;
        }
    }
}
