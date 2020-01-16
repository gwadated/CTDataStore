namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class OriginTable : PublicMiscTable
    {
        public OriginTable()
           : base("ORIGIN")
        {
            AddColumn(new BigIntColumn("origin_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new NotNullStringColumn("type"));
            AddColumn(new DateTimeColumn("last_imported"));

            AddPrimaryKey("origin_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("origin_id");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
