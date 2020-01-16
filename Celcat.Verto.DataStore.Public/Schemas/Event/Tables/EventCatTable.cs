namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventCatTable : PublicEventTable
    {
        public EventCatTable()
           : base("EVENT_CAT")
        {
            AddColumn(new BigIntColumn("event_cat_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(new IntColumn("colour"));
            AddColumn(new FloatColumn("weighting"));
            AddColumn(new BitColumn("registers_req", ColumnNullable.True));
            AddColumn(new BitColumn(ColumnConstants.RegistersReqResolvedColumnName, ColumnNullable.True));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("event_cat_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.EventCat, "event_cat_id");
            m.AddColumnMappingLookup("event_cat_id", "name", ConsolidationType.EventCat, c);
            m.AddBooleanMapping("registers_req");
            m.AddBooleanMapping(ColumnConstants.RegistersReqResolvedColumnName);
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
