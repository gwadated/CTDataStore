namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System;

    public struct EventStartEndTime
    {
        public DateTime Start { get; set; }
    
        public DateTime End { get; set; }
        
        public DayOfWeek DayOfWeek { get; set; }
    }
}
