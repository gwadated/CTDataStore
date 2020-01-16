namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class AttendTable : V7StagingTable
    {
        public AttendTable(string schemaName)
           : base("CT_AT_ATTEND", schemaName)
        {
            AddColumn(new BigIntColumn("attend_id"));
            AddColumn(new BigIntColumn("activity_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new BigIntColumn("mark_id"));
            AddColumn(new IntColumn("mins_late"));
            AddColumn(new StringColumn("comments", ColumnConstants.StrLenComments));
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new ActivityIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new MarkIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
