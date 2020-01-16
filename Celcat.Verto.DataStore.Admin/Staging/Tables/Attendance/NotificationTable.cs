namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class NotificationTable : V7StagingTable
    {
        public NotificationTable(string schemaName)
           : base("CT_AT_NOTIFICATION", schemaName)
        {
            AddColumn(new BigIntColumn("message_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new BigIntColumn("activity_id"));
            AddColumn(new StringColumn("msg_text", ColumnConstants.StrLenDescription));
            AddColumn(new Ct7BoolColumn("sent"));
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new ActivityIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
