namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SupervisorTable : V7StagingTable
    {
        public SupervisorTable(string schemaName)
           : base("CT_SUPERVISOR", schemaName)
        {
            AddColumn(new BigIntColumn("supervisor_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("business_name", ColumnConstants.StrLenStd));
            AddColumn(new Ct7TelephoneColumn("mobile"));
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7BoolColumn("can_send_sms"));
            AddColumn(new Ct7BoolColumn("can_send_email"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
