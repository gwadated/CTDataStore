namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RegisterMarkTable : V7StagingTable
    {
        public RegisterMarkTable(string schemaName)
           : base(StagingTablesBuilder.PseudoRegisterMarkTable, schemaName)
        {
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new IntColumn("week"));
            AddColumn(new BigIntColumn("mark_id"));
            AddColumn(new IntColumn("mins_late"));
            AddColumn(new StringColumn("comments", ColumnConstants.StrLenComments));
            AddColumn(new FixedCharColumn("source", 1));
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new MarkIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
