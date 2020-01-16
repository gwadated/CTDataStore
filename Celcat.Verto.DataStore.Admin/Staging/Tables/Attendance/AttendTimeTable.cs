namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AttendTimeTable : V7StagingTable
    {
        public AttendTimeTable(string schemaName)
           : base("CT_AT_ATTEND_TIME", schemaName)
        {
            AddColumn(new BigIntColumn("attend_time_id"));
            AddColumn(new BigIntColumn("activity_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new DateTimeColumn("in_time"));
            AddColumn(new DateTimeColumn("out_time"));
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new ActivityIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
