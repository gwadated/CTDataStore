namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventStaffCatReqTable : PublicEventTable
    {
        public EventStaffCatReqTable()
           : base("EVENT_STAFFCAT_REQ")
        {
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("staff_cat_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("staff_cat_name"));

            AddPrimaryKey("event_id", "staff_cat_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();

            m.AddFederatedIdMapping("event_id");
            m.AddStaffCatIdAndNameMapping(c);

            return m;
        }
    }
}
