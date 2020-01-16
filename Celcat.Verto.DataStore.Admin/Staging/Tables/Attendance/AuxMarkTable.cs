namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AuxMarkTable : V7StagingTable
    {
        public AuxMarkTable(string schemaName)
           : base("CT_AT_AUX_MARK", schemaName)
        {
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateCustomColumns(2));

            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
