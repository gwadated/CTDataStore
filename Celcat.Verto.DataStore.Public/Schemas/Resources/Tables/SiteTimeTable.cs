namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SiteTimeTable : PublicResourceTable
    {
        public SiteTimeTable()
           : base("SITE_TIME")
        {
            AddColumn(new BigIntColumn("site_id1", ColumnNullable.False));
            AddColumn(new BigIntColumn("site_id2", ColumnNullable.False));
            AddColumn(new IntColumn("travel_time", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("site_name1"));
            AddColumn(new NotNullStringColumn("site_name2"));

            AddPrimaryKey("site_id1", "site_id2");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();

            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Site, "site_id1");
            m.AddColumnMappingLookup("site_id1", "site_name1", ConsolidationType.Site, c);

            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Site, "site_id2");
            m.AddColumnMappingLookup("site_id2", "site_name2", ConsolidationType.Site, c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
