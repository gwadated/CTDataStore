namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    public class FixupCaches
    {
        private readonly NameCache _nameCache;
        private readonly EventTimeCache _eventTimeCache;
        private readonly WeekDatesCache _weekDatesCache;

        public FixupCaches(string connectionString, int timeoutSecs, TableMappings.TableMappings tableMappings)
        {
            _nameCache = new NameCache(connectionString, timeoutSecs, tableMappings);
            _eventTimeCache = new EventTimeCache(connectionString, timeoutSecs);
            _weekDatesCache = new WeekDatesCache(connectionString, timeoutSecs);
        }

        public NameCache NameCache => _nameCache;

        public EventTimeCache EventTimeCache => _eventTimeCache;
    
        public WeekDatesCache WeekDatesCache => _weekDatesCache;
    }
}
