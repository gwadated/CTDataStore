namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AuxTimeTable : V7StagingTable
    {
        public AuxTimeTable(string schemaName)
           : base("CT_AT_AUX_TIME", schemaName)
        {
            AddColumn(new BigIntColumn("activity_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new ActivityIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
