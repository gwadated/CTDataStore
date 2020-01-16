namespace Celcat.Verto.DataStore.Admin.History
{
    using System.Collections.Generic;
    using Celcat.Verto.Common;

    internal class SourceTimetableAndRowCount
    {
        public int SrcTimetableId { get; set; }

        public int RowCount { get; set; }

        public static IReadOnlyList<SourceTimetableAndRowCount> Get(
            string connectionString, int timeoutSecs, string tableName, string schemaName)
        {
            var result = new List<SourceTimetableAndRowCount>();

            var sql =
                $"select src_timetable_id, count(1) cnt from {DatabaseUtils.GetQualifiedTableName(schemaName, tableName)} group by src_timetable_id";

            DatabaseUtils.EnumerateResults(connectionString, sql, timeoutSecs, r =>
            {
                result.Add(new SourceTimetableAndRowCount
                {
                    SrcTimetableId = (int)r["src_timetable_id"],
                    RowCount = (int)r["cnt"]
                });
            });

            return result;
        }
    }
}
