namespace Celcat.Verto.DataStore.Public.Transformation.Cache
{
    using System;
    using System.Collections.Generic;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Public.Schemas.Misc;
    using Celcat.Verto.DataStore.Public.Staging;

    public class WeekDatesCache
    {
        private static readonly object _locker = new object();

        private readonly Dictionary<int, WeekDatesForTimetable> _data;
        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        
        public WeekDatesCache(string connectionString, int timeoutSecs)
        {
            _data = new Dictionary<int, WeekDatesForTimetable>();
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
        }

        public WeekDatesForTimetable Get(int srcTimetableId)
        {
            if (!_data.TryGetValue(srcTimetableId, out var result))
            {
                lock (_locker)
                {
                    if (!_data.TryGetValue(srcTimetableId, out result))
                    {
                        Populate(srcTimetableId);
                        if (!_data.TryGetValue(srcTimetableId, out result))
                        {
                            throw new ApplicationException("Unable to get week dates");
                        }
                    }
                }
            }

            return result;
        }

        private void Populate(int srcTimetableId)
        {
            if (!PopulateFromStage(srcTimetableId))
            {
                PopulateFromPublic(srcTimetableId);
            }
        }

        private void PopulateFromPublic(int srcTimetableId)
        {
            var tableName = DatabaseUtils.GetQualifiedTableName(MiscSchema.MiscSchemaName, "TIMETABLE_CONFIG");

            var sb = new SqlBuilder();
            sb.AppendFormat("select * from {0}", tableName);
            sb.AppendFormat("where timetable_id={0}", srcTimetableId);

            DatabaseUtils.GetSingleResult(_connectionString, sb.ToString(), _timeoutSecs, r =>
            {
                var dates = new WeekDatesForTimetable();

                int numWeeks = (int)r["no_weeks"];
                for (int wk = 0; wk < numWeeks; ++wk)
                {
                    var colName = $"week{wk + 1}_date";

                    if (wk == 0)
                    {
                        dates.StartingDayOfWeek = ((DateTime)r[colName]).DayOfWeek;
                    }

                    dates.StartingDates.Add((DateTime)r[colName]);
                }

                _data.Add(srcTimetableId, dates);
            });
        }

        private bool PopulateFromStage(int srcTimetableId)
        {
            bool found = false;

            string tableName = DatabaseUtils.GetQualifiedTableName(PublicStagingSchema.StagingSchemaName, "CT_CONFIG");

            SqlBuilder sb = new SqlBuilder();
            sb.AppendFormat("select * from {0}", tableName);
            sb.AppendFormat("where src_timetable_id={0}", srcTimetableId);
            sb.AppendFormat(
                "and {0} in ('{1}', '{2}')", 
                HistorySchema.HistoryStatusColumnName,
                HistorySchema.HistoryStatusInsert, 
                HistorySchema.HistoryStatusUpdate);

            DatabaseUtils.GetSingleResult(_connectionString, sb.ToString(), _timeoutSecs, r =>
            {
                var dates = new WeekDatesForTimetable();

                var numWeeks = (int)r["no_weeks"];
                for (int wk = 0; wk < numWeeks; ++wk)
                {
                    var colName = $"week{wk + 1}_date";

                    if (wk == 0)
                    {
                        dates.StartingDayOfWeek = ((DateTime)r[colName]).DayOfWeek;
                    }

                    dates.StartingDates.Add((DateTime)r[colName]);
                }

                _data.Add(srcTimetableId, dates);

                found = true;
            });

            return found;
        }
    }
}
