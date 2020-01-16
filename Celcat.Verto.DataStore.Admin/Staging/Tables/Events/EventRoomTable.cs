namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventRoomTable : V7StagingTable
    {
        public EventRoomTable(string schemaName)
           : base("CT_EVENT_ROOM", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new Ct7WeeksColumn());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new RoomLayoutIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
