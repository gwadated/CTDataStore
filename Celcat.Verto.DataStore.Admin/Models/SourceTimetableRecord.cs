namespace Celcat.Verto.DataStore.Admin.Models
{
    using System;

    internal class SourceTimetableRecord
    {
        public int Id { get; set; }
    
        public string Name { get; set; }
        
        public string SqlServerName { get; set; }
        
        public string DatabaseName { get; set; }
        
        public int SchemaVersion { get; set; }
        
        public Guid Identifier { get; set; }
    }
}
