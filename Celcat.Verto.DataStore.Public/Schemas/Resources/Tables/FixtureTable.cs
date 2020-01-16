namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class FixtureTable : PublicResourceTable
    {
        public FixtureTable()
           : base("FIXTURE")
        {
            AddColumn(new BigIntColumn("fixture_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("fixture_id");
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Fixture, "fixture_id");
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
