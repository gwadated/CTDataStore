namespace Celcat.Verto.DataStore.Admin.Models
{
    using System;

    public class SourceTimetableData
    {
        public string Name { get; set; }
    
        public string SqlServerName { get; set; }
        
        public string DatabaseName { get; set; }
        
        public int SchemaVersion { get; set; }
        
        public Guid Identifier { get; set; }
        
        public string ConnectionString { get; set; }
    }
}
