namespace Celcat.Verto.DataStore.Admin.History
{
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.TableDiff;

    internal class StageTableDiffer : ResultSetDiffer
    {
        public StageTableDiffer(
            string connectionString, 
            int commandTimeoutSecs,
            string oldTable, 
            string newTable, 
            int timetableId, 
            int primaryKeyColCount = 1)
           : base(
               connectionString, 
               commandTimeoutSecs,
               $"select * from {DatabaseUtils.EscapeDbObject(oldTable)} where src_timetable_id = {timetableId}",
               $"select * from {DatabaseUtils.EscapeDbObject(newTable)} where src_timetable_id = {timetableId}",
               primaryKeyColCount)
        {
        }
    }
}
