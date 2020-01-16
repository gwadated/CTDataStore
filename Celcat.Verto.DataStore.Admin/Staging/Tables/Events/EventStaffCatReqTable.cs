namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventStaffCatReqTable : V7StagingTable
    {
        public EventStaffCatReqTable(string schemaName)
           : base("CT_EVENT_STAFFCAT_REQ", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("staff_cat_id"));

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new StaffCatIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
