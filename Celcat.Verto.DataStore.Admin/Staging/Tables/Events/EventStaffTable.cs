namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventStaffTable : V7StagingTable
    {
        public EventStaffTable(string schemaName)
           : base("CT_EVENT_STAFF", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(new BigIntColumn("staff_cat_id"));
            AddColumn(new Ct7WeeksColumn());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new StaffIdReferenceCheck());
            AddColumnReferenceCheck(new StaffCatIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
