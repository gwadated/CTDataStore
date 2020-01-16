namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventLayoutReqTable : V7StagingTable
    {
        public EventLayoutReqTable(string schemaName)
           : base("CT_EVENT_LAYOUT_REQ", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("room_layout_id"));

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new RoomLayoutIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
