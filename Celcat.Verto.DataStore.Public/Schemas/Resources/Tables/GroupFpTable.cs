namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupFpTable : PublicResourceTable
    {
        public GroupFpTable()
           : base("GROUP_FP")
        {
            AddColumn(new BigIntColumn("group_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(new FixedCharColumn("fix_pref", 1, ColumnNullable.False));
            AddColumn(new IntColumn("preference"));
            AddColumn(new Ct7UniqueNameColumn("group_unique_name"));
            AddColumn(new Ct7NameColumn("group_name"));

            AddPrimaryKey("group_id", "resource_type", "resource_id", "fix_pref");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddGroupIdAndNameMapping(c);
            m.AddResourceIdAndNameMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
