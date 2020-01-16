namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System;
    using System.Collections.Generic;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Schemas.Event;
    using Celcat.Verto.DataStore.Public.Staging;

    public class EventTimeCache
    {
        private static readonly object _locker = new object();
        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private Dictionary<long, EventStartEndTime> _eventTimes;

        public EventTimeCache(string connectionString, int timeoutSecs)
        {
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
        }

        public EventStartEndTime Get(long eventId)
        {
            if (_eventTimes == null)
            {
                lock (_locker)
                {
                    if (_eventTimes == null)
                    {
                        _eventTimes = new Dictionary<long, EventStartEndTime>();
                        Populate();
                    }
                }
            }

            _eventTimes.TryGetValue(eventId, out var result);

            return result;
        }

        private void Populate()
        {
            PopulateFromStagingTable();
            PopulateFromPublicTable();
        }

        private void PopulateFromPublicTable()
        {
            string publicEventTable = DatabaseUtils.GetQualifiedTableName(EventSchema.EventSchemaName, EntityUtils.ToFederationTableName(Entity.Event));

            var s = new SqlBuilder();
            s.AppendFormat("select event_id, day_of_week, start_time, end_time from {0}", publicEventTable);

            DatabaseUtils.EnumerateResults(_connectionString, s.ToString(), _timeoutSecs, r =>
            {
                var eventId = (long)r["event_id"];

                if (!_eventTimes.ContainsKey(eventId))
                {
                    var times = new EventStartEndTime
                    {
                        Start = (DateTime)r["start_time"],
                        End = (DateTime)r["end_time"],
                        DayOfWeek = Utils.ConvertFromCt7DayOfWeek((int)r["day_of_week"])
                    };

                    _eventTimes.Add(eventId, times);
                }
            });
        }

        private void PopulateFromStagingTable()
        {
            var stagingTableName = DatabaseUtils.GetQualifiedTableName(
                PublicStagingSchema.StagingSchemaName, EntityUtils.ToCtTableName(Entity.Event));

            string federatedEventIdCol = EntityUtils.GetFederatedFieldName("event_id");

            var s = new SqlBuilder();
            s.AppendFormat("select {0}, day_of_week, start_time, end_time from {1}", federatedEventIdCol, stagingTableName);
            s.AppendFormat(
                "where {0} in ('{1}', '{2}')", 
                HistorySchema.HistoryStatusColumnName,
                HistorySchema.HistoryStatusInsert, 
                HistorySchema.HistoryStatusUpdate);

            DatabaseUtils.EnumerateResults(_connectionString, s.ToString(), _timeoutSecs, r =>
            {
                var times = new EventStartEndTime
                {
                    Start = (DateTime)r["start_time"],
                    End = (DateTime)r["end_time"],
                    DayOfWeek = Utils.ConvertFromCt7DayOfWeek((int)r["day_of_week"])
                };

                var eventId = (long)r[federatedEventIdCol];
                _eventTimes.Add(eventId, times);
            });
        }
    }
}
