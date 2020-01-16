namespace Celcat.Verto.DataStore.Public.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas.Attendance;
    using Celcat.Verto.DataStore.Public.Schemas.Booking;
    using Celcat.Verto.DataStore.Public.Schemas.Event;
    using Celcat.Verto.DataStore.Public.Schemas.Exam;
    using Celcat.Verto.DataStore.Public.Schemas.Membership;
    using Celcat.Verto.DataStore.Public.Schemas.Misc;
    using Celcat.Verto.DataStore.Public.Schemas.Resources;
    using Celcat.Verto.DataStore.Public.Schemas.TempUpsert;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;

    public abstract class PublicTable : Table
    {
        private const int EventInstanceParamNum = 10000;
        private const int EventWeekParamNum = 10001;
        private const int EventWeekOccurrenceParamNum = 10002;

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected PublicTable(string tableName, string schemaName)
           : base(tableName, schemaName)
        {
        }

        private string GenerateIndexName(string colName)
        {
            return $"IX_{Name}_{colName}";
        }

        protected void AddNameIndex()
        {
            const string colName = "name";
            AddIndex(GenerateIndexName(colName), false, colName);
        }

        protected void AddUniqueNameIndex()
        {
            const string colName = "unique_name";
            AddIndex(GenerateIndexName(colName), false, colName);
        }

        public abstract TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c);

        private string GetMergeSelectCols(TableColumnMappings colMappings)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < colMappings.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetParamName(n));
                sb.Append(" as ");
                sb.Append(colMappings[n].PublicColumn);
            }

            if (colMappings.EventExpansionRequired)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetParamName(EventInstanceParamNum));
                sb.Append(" as ");
                sb.Append(colMappings.EventExpansion.PublicEventInstanceColumn);

                sb.Append(",");

                sb.Append(GetParamName(EventWeekParamNum));
                sb.Append(" as ");
                sb.Append(colMappings.EventExpansion.PublicWeekColumn);

                sb.Append(",");

                sb.Append(GetParamName(EventWeekOccurrenceParamNum));
                sb.Append(" as ");
                sb.Append(colMappings.EventExpansion.PublicWeekOccurrenceColumn);
            }

            return sb.ToString();
        }

        private string GenerateTempUpsertTable(
            SqlConnection c, 
            int timeoutSecs, 
            IEnumerable<Row> stagingRows,
            TableColumnMappings colMappings, 
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            bool modifyWeekNumbers)
        {
            var tableName = string.Concat("#TmpUpsert", Name);

            var b = new TempUpsertTableBuilder(tableName, this);
            b.Execute(c, null, timeoutSecs);
            var tmpTable = b.GetTables()[0];

            PopulateTempUpsertTableUsingBulkCopy(c, timeoutSecs, stagingRows, colMappings, caches, configuration, tmpTable, modifyWeekNumbers);

            return tableName;
        }

        private void PopulateTempUpsertTableUsingBulkCopy(
            SqlConnection c, 
            int timeoutSecs, 
            IEnumerable<Row> stagingRows,
            TableColumnMappings colMappings, 
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            Table tableName,
            bool modifyWeekNumbers)
        {
            var etl = new TempUpsertEtlProcess(c, timeoutSecs, stagingRows, colMappings, caches, configuration, tableName, modifyWeekNumbers);
            etl.Execute();

            var errors = etl.GetAllErrors().ToArray();
            if (errors.Any())
            {
                string msg = $"Errors occurred during population of temporary upsert table: {tableName.Name}";
                _log.Error(msg);

                // throw the first exception
                throw new ApplicationException(msg, errors[0]);
            }
        }

        public void Upsert(
            string sqlConnectionString, 
            int timeoutSecs, 
            IEnumerable<Row> stagingRows,
            TableColumnMappings colMappings,
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            bool modifyWeekNumbers = false)
        {
            using (var c = DatabaseUtils.CreateConnection(sqlConnectionString))
            {
                var tmpTableName = GenerateTempUpsertTable(c, timeoutSecs, stagingRows, colMappings, caches, configuration, modifyWeekNumbers);

                var b = new SqlBuilder();

                b.AppendFormat("MERGE {0} target", QualifiedName);
                b.AppendFormat("using (select {0} from {1}) source", colMappings.GetPublicColNamesAsCsv(), tmpTableName);
                b.Append("on (");

                for (int n = 0; n < PrimaryKey.KeyPartsCount; ++n)
                {
                    if (n > 0)
                    {
                        b.Append("and");
                    }

                    var keyPart = PrimaryKey[n];
                    b.AppendFormat("target.{0} = source.{0}", keyPart.ColumnName);
                }

                b.Append(")");

                b.Append("when MATCHED then");
                b.Append("UPDATE SET");

                for (int n = 0; n < Columns.Count; ++n)
                {
                    if (n > 0)
                    {
                        b.AppendNoSpace(", ");
                    }

                    var col = Columns[n];
                    b.AppendFormat("target.{0} = source.{0}", col.Name);
                }

                b.Append("when NOT MATCHED then");
                b.AppendFormat("INSERT ({0}) values({1})", colMappings.GetPublicColNamesAsCsv(), GetSourceColsNamesAsCsv(colMappings));
                b.Append(";");

                DatabaseUtils.ExecuteSql(c, null, b.ToString(), timeoutSecs);
            }
        }

        private string GetSourceColsNamesAsCsv(TableColumnMappings colMappings)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < colMappings.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(",");
                }

                sb.Append("source.");
                sb.Append(colMappings[n].PublicColumn);
            }

            if (colMappings.EventExpansionRequired)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append("source.");
                sb.Append(colMappings.EventExpansion.PublicEventInstanceColumn);

                sb.Append(",");
                sb.Append("source.");
                sb.Append(colMappings.EventExpansion.PublicWeekColumn);

                sb.Append(",");
                sb.Append("source.");
                sb.Append(colMappings.EventExpansion.PublicWeekOccurrenceColumn);
            }

            return sb.ToString();
        }

        public void Upsert(
            string sqlConnectionString, 
            int timeoutSecs, 
            Row stagingRow, 
            TableColumnMappings colMappings,
            FixupCaches caches, 
            DataStoreConfiguration configuration)
        {
            var b = new SqlBuilder();

            b.AppendFormat("MERGE {0} a", QualifiedName);
            b.AppendFormat("using (select {0}) b", GetMergeSelectCols(colMappings));
            b.Append("on (");

            for (int n = 0; n < PrimaryKey.KeyPartsCount; ++n)
            {
                if (n > 0)
                {
                    b.Append("and");
                }

                var keyPart = PrimaryKey[n];
                b.AppendFormat("a.{0}={1}", keyPart.ColumnName, GetParamName(colMappings, keyPart.ColumnName));
            }

            b.Append(")");

            b.Append("when MATCHED then");
            b.Append("UPDATE SET");

            for (int n = 0; n < Columns.Count; ++n)
            {
                if (n > 0)
                {
                    b.AppendNoSpace(", ");
                }

                var col = Columns[n];
                b.AppendFormat("{0}={1}", col.Name, GetParamName(colMappings, col.Name));
            }

            b.Append("when NOT MATCHED then");
            b.AppendFormat("INSERT ({0}) values({1})", colMappings.GetPublicColNamesAsCsv(), GetParamNamesAsCsv(colMappings));

            b.Append(";");

            int wkParamIndex = -1;

            var parameters = new List<SqlParameter>();
            for (int n = 0; n < colMappings.Count; ++n)
            {
                string paramName = GetParamName(n);
                colMappings[n].AddParamValue(parameters, paramName, stagingRow, caches, configuration);

                if (colMappings[n].PublicColumn.Equals("timetable_week"))
                {
                    wkParamIndex = n;
                }
            }

            if (wkParamIndex > -1)
            {
                // make the "timetable_week" 1-based...
                parameters[wkParamIndex].Value = (int)parameters[wkParamIndex].Value + 1;
            }

            if (colMappings.EventExpansionRequired)
            {
                string paramName1 = GetParamName(EventInstanceParamNum);
                parameters.Add(new SqlParameter(paramName1, stagingRow[colMappings.EventExpansion.PublicEventInstanceColumn]));

                string paramName2 = GetParamName(EventWeekParamNum);
                parameters.Add(new SqlParameter(paramName2, stagingRow[colMappings.EventExpansion.PublicWeekColumn]));

                string paramName3 = GetParamName(EventWeekOccurrenceParamNum);
                parameters.Add(new SqlParameter(paramName3, stagingRow[colMappings.EventExpansion.PublicWeekOccurrenceColumn]));
            }

            DatabaseUtils.ExecuteSql(sqlConnectionString, b.ToString(), timeoutSecs, parameters.ToArray());
        }
        
        public virtual void Delete(
            string sqlConnectionString, 
            int timeoutSecs, 
            Row stagingRow, 
            TableColumnMappings colMappings,
            FixupCaches caches, 
            DataStoreConfiguration configuration)
        {
            var b = new SqlBuilder();

            b.AppendFormat("delete from {0}", QualifiedName);
            b.Append("where");

            for (int n = 0; n < PrimaryKey.KeyPartsCount; ++n)
            {
                if (n > 0)
                {
                    b.Append("and");
                }

                var keyPart = PrimaryKey[n];
                b.AppendFormat("{0}={1}", keyPart.ColumnName, GetParamName(colMappings, keyPart.ColumnName));
            }

            var parameters = new List<SqlParameter>();

            for (int n = 0; n < colMappings.Count; ++n)
            {
                string paramName = GetParamName(n);
                colMappings[n].AddParamValue(parameters, paramName, stagingRow, caches, configuration);
            }

            DatabaseUtils.ExecuteSql(sqlConnectionString, b.ToString(), timeoutSecs, parameters.ToArray());
        }

        private string GetParamName(int zeroBasedNum, string prefix = null)
        {
            return string.Concat("@P", prefix, (zeroBasedNum + 1).ToString());
        }

        private string GetParamName(TableColumnMappings colMappings, string publicColName)
        {
            for (int n = 0; n < colMappings.Count; ++n)
            {
                if (colMappings[n].PublicColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetParamName(n);
                }
            }

            if (colMappings.EventExpansionRequired)
            {
                if (colMappings.EventExpansion.PublicEventInstanceColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetParamName(EventInstanceParamNum);
                }

                if (colMappings.EventExpansion.PublicWeekColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetParamName(EventWeekParamNum);
                }

                if (colMappings.EventExpansion.PublicWeekOccurrenceColumn.Equals(publicColName, StringComparison.OrdinalIgnoreCase))
                {
                    return GetParamName(EventWeekOccurrenceParamNum);
                }
            }

            throw new ApplicationException($"Could not find column name in mapping: {publicColName}");
        }

        private string GetParamNamesAsCsv(TableColumnMappings colMappings, string prefix = null)
        {
            var sb = new StringBuilder();

            for (int n = 0; n < colMappings.Count; ++n)
            {
                if (n > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetParamName(n, prefix));
            }

            if (colMappings.EventExpansionRequired)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetParamName(EventInstanceParamNum, prefix));
                sb.Append(",");
                sb.Append(GetParamName(EventWeekParamNum, prefix));
            }

            return sb.ToString();
        }

        public void DeleteEventInstances(string sqlConnectionString, int timeoutSecs, long eventId)
        {
            string sql = $"delete from {QualifiedName} where event_id={eventId}";
            DatabaseUtils.ExecuteSql(sqlConnectionString, sql, timeoutSecs);
        }

        internal void DeleteEventInstances(string sqlConnectionString, int timeoutSecs, List<long> eventIds)
        {
            var batcher = new ItemBatcher<long>(eventIds, 250);

            var batch = batcher.GetBatch();
            while (batch != null)
            {
                var sql = $"delete from {QualifiedName} where event_id in ({batcher.GetAsCsv(batch)})";
                DatabaseUtils.ExecuteSql(sqlConnectionString, sql, timeoutSecs);

                batch = batcher.GetBatch();
            }
        }

        internal void DeleteSpanInstances(string sqlConnectionString, int timeoutSecs, List<long> spanIds)
        {
            var batcher = new ItemBatcher<long>(spanIds, 250);

            var batch = batcher.GetBatch();
            while (batch != null)
            {
                var sql = $"delete from {QualifiedName} where span_id in ({batcher.GetAsCsv(batch)})";
                DatabaseUtils.ExecuteSql(sqlConnectionString, sql, timeoutSecs);

                batch = batcher.GetBatch();
            }
        }

        public void DeleteOrphanedEventInstances(string connectionString, int timeoutSecs)
        {
            var sb = new SqlBuilder();

            sb.AppendFormat("delete from {0}", QualifiedName);
            sb.AppendFormat(
                "where event_id not in (select event_id from {0})", 
                DatabaseUtils.GetQualifiedTableName(EventSchema.EventSchemaName, "EVENT"));

            DatabaseUtils.ExecuteSql(connectionString, sb.ToString(), timeoutSecs);
        }

        public bool IsEmpty(string connectionString, int timeout)
        {
            var result = false;

            var sql = $"select count(1) from {QualifiedName}";
            DatabaseUtils.GetSingleResult(connectionString, sql, timeout, r =>
            {
                result = (int)r[0] == 0;
            });

            return result;
        }

        public static IEnumerable<Table> GetAllTables()
        {
            var result = new List<Table>();

            result.AddRange(new PublicTablesBuilder<PublicResourceTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicAttendanceTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicBookingTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicEventTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicExamTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicMembershipTable>().GetTables());
            result.AddRange(new PublicTablesBuilder<PublicMiscTable>().GetTables());

            return result;
        }
    }

    public abstract class PublicResourceTable : PublicTable
    {
        protected PublicResourceTable(string tableName)
           : base(tableName, ResourceSchema.ResourceSchemaName)
        {
        }
    }

    public abstract class PublicAttendanceTable : PublicTable
    {
        protected PublicAttendanceTable(string tableName)
           : base(tableName, AttendanceSchema.AttendanceSchemaName)
        {
        }
    }

    public abstract class PublicBookingTable : PublicTable
    {
        protected PublicBookingTable(string tableName)
           : base(tableName, BookingSchema.BookingSchemaName)
        {
        }
    }

    public abstract class PublicEventTable : PublicTable
    {
        protected PublicEventTable(string tableName)
           : base(tableName, EventSchema.EventSchemaName)
        {
        }

        protected void DeleteEventAssignment(string sqlConnectionString, int timeoutSecs, Row stagingRow)
        {
            var b = new SqlBuilder();

            long eventId = (long)stagingRow["event_id"];
            var parameters = new List<SqlParameter>();

            b.AppendFormat("delete from {0}", QualifiedName);
            b.Append("where");

            for (int n = 0; n < PrimaryKey.KeyPartsCount; ++n)
            {
                if (n > 0)
                {
                    b.Append("and");
                }

                var keyPart = PrimaryKey[n];
                var colName = keyPart.ColumnName;
                string paramName = $"@P{n + 1}";

                if (colName.Equals("event_instance_id", StringComparison.OrdinalIgnoreCase))
                {
                    b.AppendFormat("{0} like {1}", colName, paramName);
                    parameters.Add(new SqlParameter(paramName, $"{eventId}-%"));
                }
                else
                {
                    b.AppendFormat("{0}={1}", colName, paramName);
                    parameters.Add(new SqlParameter(paramName, (long)stagingRow[colName]));
                }
            }

            DatabaseUtils.ExecuteSql(sqlConnectionString, b.ToString(), timeoutSecs, parameters.ToArray());
        }
    }
    
    public abstract class PublicExamTable : PublicTable
    {
        protected PublicExamTable(string tableName)
           : base(tableName, ExamSchema.ExamSchemaName)
        {
        }
    }

    public abstract class PublicMembershipTable : PublicTable
    {
        protected PublicMembershipTable(string tableName)
           : base(tableName, MembershipSchema.MembershipSchemaName)
        {
        }
    }

    public abstract class PublicMiscTable : PublicTable
    {
        protected PublicMiscTable(string tableName)
           : base(tableName, MiscSchema.MiscSchemaName)
        {
        }
    }
}
