namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventLayoutReqTable : PublicEventTable
    {
        public EventLayoutReqTable()
           : base("EVENT_LAYOUT_REQ")
        {
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("room_layout_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("room_layout_name"));

            AddPrimaryKey("event_id", "room_layout_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();

            m.AddFederatedIdMapping("event_id");
            m.AddRoomLayoutIdAndNameMapping(c);

            return m;
        }
    }
}
