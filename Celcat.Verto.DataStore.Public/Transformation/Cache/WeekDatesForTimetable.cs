namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System;
    using System.Collections.Generic;

    public class WeekDatesForTimetable
    {
        private readonly List<DateTime> _startingDates;
    
        public DayOfWeek StartingDayOfWeek { get; set; }
        
        public List<DateTime> StartingDates => _startingDates;

        public WeekDatesForTimetable()
        {
            _startingDates = new List<DateTime>();
        }
    }
}
