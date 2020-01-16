namespace Celcat.Verto.DataStore.Public.Schemas.Membership.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class FlatGroupSubGroupTable : PublicMembershipTable
    {
        public FlatGroupSubGroupTable()
           : base("FLAT_GROUP_SUBGROUP")
        {
            AddColumn(new BigIntColumn("group_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("subgroup_id", ColumnNullable.False));
            AddColumn(new IntColumn("strength", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("group_unique_name"));
            AddColumn(new NullStringColumn("group_name"));
            AddColumn(new NotNullStringColumn("subgroup_unique_name"));
            AddColumn(new NullStringColumn("subgroup_name"));

            AddPrimaryKey("group_id", "subgroup_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddGroupIdAndNameMapping(c);
            m.AddSubGroupIdAndNameMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
