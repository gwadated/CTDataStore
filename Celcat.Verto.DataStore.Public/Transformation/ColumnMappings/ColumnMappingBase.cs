namespace Celcat.Verto.DataStore.Public.Transformation.ColumnMappings
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Rhino.Etl.Core;

    public abstract class ColumnMappingBase
    {
        public string PublicColumn { get; set; }

        public abstract void AddParamValue(
            List<SqlParameter> parameters, 
            string paramName, 
            Row stagingRow,
            FixupCaches caches, 
            DataStoreConfiguration config);

        public abstract object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config);
    }

    public class ColumnMappingStandard : ColumnMappingBase
    {
        public string StagingColumn { get; set; }

        public ColumnMappingStandard(string publicColumn, string stagingColumn = null)
        {
            PublicColumn = publicColumn;
            StagingColumn = stagingColumn ?? publicColumn;
        }

        public override void AddParamValue(
            List<SqlParameter> parameters, 
            string paramName, 
            Row stagingRow,
            FixupCaches caches, 
            DataStoreConfiguration config)
        {
            var val = GetStagingValue(stagingRow, caches, config);

            if (val == null || val == DBNull.Value)
            {
                parameters.Add(new SqlParameter(paramName, DBNull.Value));
            }
            else
            {
                parameters.Add(new SqlParameter(paramName, val));
            }
        }

        public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            return stagingRow[StagingColumn];
        }
    }

    public class ColumnMappingResourceId : ColumnMappingBase
    {
        public string FederatedStagingColumn { get; set; }
        
        public string ConsolidatedStagingColumn { get; set; }
        
        public string PublicStagingTypeColumn { get; set; }

        public ColumnMappingResourceId(
            string publicColumn, string publicStagingTypeColumn, string federatedStagingCol, string consolidatedStagingCol)
        {
            PublicColumn = publicColumn;
            FederatedStagingColumn = federatedStagingCol;
            ConsolidatedStagingColumn = consolidatedStagingCol;
            PublicStagingTypeColumn = publicStagingTypeColumn;
        }

        public override void AddParamValue(
            List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            object id = GetStagingValue(stagingRow, caches, config);

            if (id == null || id == DBNull.Value)
            {
                parameters.Add(new SqlParameter(paramName, DBNull.Value));
            }
            else
            {
                parameters.Add(new SqlParameter(paramName, id));
            }
        }

        public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            Ct7Entity resType = (Ct7Entity)stagingRow[PublicStagingTypeColumn];
            Entity entityType = EntityUtils.FromCt7Entity(resType);

            return config.Consolidation.Get(entityType).None
               ? stagingRow[FederatedStagingColumn]
               : stagingRow[ConsolidatedStagingColumn];
        }
    }

    public class ColumnMappingEventStartEndTime : ColumnMappingBase
    {
        public ColumnMappingEventStartEndTime(string colName)
        {
            PublicColumn = colName;
        }

        public override void AddParamValue(
            List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            DateTime dt = (DateTime)GetStagingValue(stagingRow, caches, config);
            parameters.Add(new SqlParameter(paramName, dt));
        }

        public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            long eventId = (long)stagingRow["federated_event_id"];
            int srcTimetableId = (int)stagingRow["src_timetable_id"];
            int timetableWeek = (int)stagingRow["timetable_week"];   // this column will have been added by event expansion

            var eventTimes = caches.EventTimeCache.Get(eventId);
            var weekDates = caches.WeekDatesCache.Get(srcTimetableId);

            DateTime dateOfEvent = weekDates.StartingDates[timetableWeek - 1];
            while (dateOfEvent.DayOfWeek != eventTimes.DayOfWeek)
            {
                dateOfEvent = dateOfEvent.AddDays(1);
            }

            DateTime eventInstanceDate = dateOfEvent.Date;

            DateTime dt = PublicColumn.IndexOf("start", StringComparison.OrdinalIgnoreCase) >= 0
               ? eventInstanceDate.AddHours(eventTimes.Start.Hour).AddMinutes(eventTimes.Start.Minute)
               : eventInstanceDate.AddHours(eventTimes.End.Hour).AddMinutes(eventTimes.End.Minute);

            return dt;
        }
    }

    public class ColumnMappingLookup : ColumnMappingBase
    {
        private readonly Entity _entityType;

        public string StagingIdColumn { get; set; }

        public string PublicIdColumn { get; set; }

        public ColumnMappingLookup(string stagingIdColumn, string publicIdColumn, string publicNameColumn, Entity eType)
        {
            _entityType = eType;
            StagingIdColumn = stagingIdColumn;
            PublicIdColumn = publicIdColumn;
            PublicColumn = publicNameColumn;
        }

        public override void AddParamValue(
            List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            object id = GetStagingValue(stagingRow, caches, config);
            parameters.Add(new SqlParameter(paramName, id));
        }

        public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
        {
            object id = stagingRow[StagingIdColumn];
            if (id == null || id == DBNull.Value)
            {
                return DBNull.Value;
            }

            long idVal = (long)id;
            var name = caches.NameCache.Get(_entityType, idVal, config);
            if (name != null)
            {
                if (PublicColumn.Contains("unique"))
                {
                    if (string.IsNullOrEmpty(name.UniqueName))
                    {
                        throw new ApplicationException("Unique name is empty!");
                    }

                    return name.UniqueName;
                }

                if (string.IsNullOrEmpty(name.Name))
                {
                    return DBNull.Value;
                }

                return name.Name;
            }

            return DBNull.Value;
        }

        public class ColumnMappingResourceLookup : ColumnMappingBase
        {
            public string FederatedStagingIdColumn { get; set; }
            
            public string ConsolidatedStagingIdColumn { get; set; }
            
            public string StagingTypeColumn { get; set; }
            
            public string PublicIdColumn { get; set; }

            public ColumnMappingResourceLookup(
                string federatedStagingIdColumn, 
                string consolidatedStagingIdColumn,
                string stagingResourceTypeColumn, 
                string publicIdColumn, 
                string publicNameColumn)
            {
                FederatedStagingIdColumn = federatedStagingIdColumn;
                ConsolidatedStagingIdColumn = consolidatedStagingIdColumn;
                StagingTypeColumn = stagingResourceTypeColumn;
                PublicIdColumn = publicIdColumn;
                PublicColumn = publicNameColumn;
            }

            public override void AddParamValue(
                List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                object val = GetStagingValue(stagingRow, caches, config);
                parameters.Add(new SqlParameter(paramName, val));
            }

            public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                Ct7Entity resType = (Ct7Entity)stagingRow[StagingTypeColumn];
                Entity entityType = EntityUtils.FromCt7Entity(resType);

                object id = config.Consolidation.Get(entityType).None
                   ? stagingRow[FederatedStagingIdColumn]
                   : stagingRow[ConsolidatedStagingIdColumn];

                if (id == null || id == DBNull.Value)
                {
                    return DBNull.Value;
                }

                long idVal = (long)id;
                var name = caches.NameCache.Get(entityType, idVal, config);
                if (name != null)
                {
                    if (PublicColumn.Contains("unique"))
                    {
                        return name.UniqueName;
                    }

                    if (string.IsNullOrEmpty(name.Name))
                    {
                        return DBNull.Value;
                    }

                    return name.Name;
                }

                return DBNull.Value;
            }
        }

        public class ColumnMappingEventInstance : ColumnMappingBase
        {
            public string StagingEventIdColumn { get; set; }
            
            public string StagingEventWeekColumn { get; set; }
            
            public ColumnMappingEventInstance(string stagingEventIdColumn, string stagingEventWeekColumn)
            {
                PublicColumn = "event_instance_id";
                StagingEventIdColumn = stagingEventIdColumn;
                StagingEventWeekColumn = stagingEventWeekColumn;
            }

            public static string FabricateEventInstanceId(long eventId, int zeroBasedWeek)
            {
                return $"{eventId}-{zeroBasedWeek + 1:D2}";
            }

            public override void AddParamValue(
                List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                var val = GetStagingValue(stagingRow, caches, config);
                parameters.Add(new SqlParameter(paramName, val));
            }

            public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                long eventId = (long)stagingRow[StagingEventIdColumn];
                int week = (int)stagingRow[StagingEventWeekColumn];
                return FabricateEventInstanceId(eventId, week);
            }
        }
        
        public class ColumnMappingBoolean : ColumnMappingBase
        {
            public string StagingColumn { get; set; }

            public ColumnMappingBoolean(string publicColumn, string stagingColumn = null)
            {
                PublicColumn = publicColumn;
                StagingColumn = stagingColumn ?? publicColumn;
            }

            public override void AddParamValue(
                List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                object val = GetStagingValue(stagingRow, caches, config);
                parameters.Add(new SqlParameter(paramName, val));
            }

            public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                object val = stagingRow[StagingColumn];
                if (val == null || val == DBNull.Value)
                {
                    return DBNull.Value;
                }

                return ((string)val).Equals("Y", StringComparison.OrdinalIgnoreCase)
                   ? 1 : 0;
            }
        }

        // used for 'enumerations'
        public abstract class ColumnMappingHardCodedLookup : ColumnMappingBase
        {
            public string StagingColumn { get; set; }

            public ColumnMappingHardCodedLookup(string publicColumn, string stagingColumn)
            {
                PublicColumn = publicColumn;
                StagingColumn = stagingColumn;
            }

            protected abstract string GetHardCodedValue(string enumValue);

            public override void AddParamValue(
                List<SqlParameter> parameters, string paramName, Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                var val = GetStagingValue(stagingRow, caches, config);
                parameters.Add(new SqlParameter(paramName, val));
            }

            public override object GetStagingValue(Row stagingRow, FixupCaches caches, DataStoreConfiguration config)
            {
                var val = stagingRow[StagingColumn];
                if (val != null && val != DBNull.Value)
                {
                    return GetHardCodedValue((string)val);
                }

                return DBNull.Value;
            }
        }

        public class ColumnMappingMarkDefinition : ColumnMappingHardCodedLookup
        {
            public ColumnMappingMarkDefinition(string publicColumn, string stagingColumn)
               : base(publicColumn, stagingColumn)
            {
            }

            protected override string GetHardCodedValue(string enumValue)
            {
                switch (enumValue.ToUpper())
                {
                    case "P":
                        return "Present";
                    case "A":
                        return "Absent";
                    case "W":
                        return "Withdrawn";
                    case "L":
                        return "Late";

                    default:
                        return string.Empty;
                }
            }
        }
    }
}
