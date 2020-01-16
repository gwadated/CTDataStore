namespace Celcat.Verto.DataStore.Common.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Serialization;
    using Celcat.Verto.DataStore.Common.Entities;

    [XmlType("consolidation")]
    public class ConsolidationParams
    {
        public const string NONE = "none";

        [XmlAttribute("enabled")]
        public bool Enabled { get; set; }

        [XmlElement("entry")]
        public List<ConsolidationEntry> Entries { get; set; }

        public ConsolidationParams()
        {
            Enabled = true;
            Entries = new List<ConsolidationEntry>();
        }

        public void ClearEntries()
        {
            foreach (var entry in Entries)
            {
                entry.Column = NONE;
            }
        }

        public ConsolidationEntry Get(Entity e)
        {
            return 
               Entries.FirstOrDefault(x => x.Entity.Equals(e.ToString(), StringComparison.OrdinalIgnoreCase))
               ??
               new ConsolidationEntry
               {
                   Entity = e.ToString(),
                   Column = NONE
               };
        }

        public bool DiffersFrom(ConsolidationParams c)
        {
            if (Enabled != c.Enabled)
            {
                return true;
            }

            if (Enabled)
            {
                foreach (Entity e in Enum.GetValues(typeof(Entity)))
                {
                    if (Get(e).DiffersFrom(c.Get(e)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
