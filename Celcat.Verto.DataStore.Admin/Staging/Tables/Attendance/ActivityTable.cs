namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class ActivityTable : V7StagingTable
    {
        public ActivityTable(string schemaName)
           : base("CT_AT_ACTIVITY", schemaName)
        {
            AddColumn(new BigIntColumn("activity_id"));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new IntColumn("week"));
            AddColumn(new DateTimeColumn("start_datetime"));
            AddColumn(new DateTimeColumn("end_datetime"));
            AddColumn(new Ct7BoolColumn("closed"));
            AddColumn(new Ct7NotesColumn());
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(new Ct7BoolColumn("staff_present"));
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new StaffIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
