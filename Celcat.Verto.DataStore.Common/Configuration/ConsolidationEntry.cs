namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System;
    using System.Xml.Serialization;

    [XmlType("consolidationEntry")]
    public class ConsolidationEntry
    {
        [XmlAttribute("entity")]
        public string Entity { get; set; }

        [XmlAttribute("column")]
        public string Column { get; set; }

        public bool None => Column.Equals(ConsolidationParams.NONE, StringComparison.OrdinalIgnoreCase);

        public bool DiffersFrom(ConsolidationEntry entry)
        {
            return
               !Entity.Equals(entry.Entity, StringComparison.OrdinalIgnoreCase) ||
               !Column.Equals(entry.Column, StringComparison.OrdinalIgnoreCase);
        }
    }
}
