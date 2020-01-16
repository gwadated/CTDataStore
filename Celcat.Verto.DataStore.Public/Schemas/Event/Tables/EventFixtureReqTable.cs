namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventFixtureReqTable : PublicEventTable
    {
        public EventFixtureReqTable()
           : base("EVENT_FIXTURE_REQ")
        {
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("fixture_id", ColumnNullable.False));
            AddColumn(new IntColumn("quantity", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("fixture_name"));

            AddPrimaryKey("event_id", "fixture_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();

            m.AddFederatedIdMapping("event_id");
            m.AddFixtureIdAndNameMapping(c);
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
