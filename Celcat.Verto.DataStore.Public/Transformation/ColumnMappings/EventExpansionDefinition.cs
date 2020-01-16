namespace Celcat.Verto.DataStore.Public.Transformation.ColumnMappings
{
    using System;

    public class EventExpansionDefinition
    {
        public string StagingFederatedEventIdColumn { get; set; }
    
        public string StagingWeeksColumn { get; set; }
        
        public string PublicEventInstanceColumn { get; set; }
        
        public string PublicWeekColumn { get; set; }

        public string PublicWeekOccurrenceColumn { get; set; }
        
        public static EventExpansionDefinition Standard =>
           new EventExpansionDefinition
           {
               StagingFederatedEventIdColumn = "federated_event_id",
               StagingWeeksColumn = "weeks",
               PublicEventInstanceColumn = "event_instance_id",
               PublicWeekColumn = "timetable_week",
               PublicWeekOccurrenceColumn = "timetable_occurrence"
           };

        public int PublicColumnCount
        {
            get
            {
                int count = 0;
                if (!string.IsNullOrEmpty(PublicEventInstanceColumn))
                {
                    ++count;
                }

                if (!string.IsNullOrEmpty(PublicWeekColumn))
                {
                    ++count;
                }

                if (!string.IsNullOrEmpty(PublicWeekOccurrenceColumn))
                {
                    ++count;
                }

                return count;
            }
        }

        public bool PublicColumnSpecified(string publicColName)
        {
            return
               PublicEventInstanceColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase) ||
               PublicWeekColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase) ||
               PublicWeekOccurrenceColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
