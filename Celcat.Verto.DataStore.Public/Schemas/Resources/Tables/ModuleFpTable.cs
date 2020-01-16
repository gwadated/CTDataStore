namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ModuleFpTable : PublicResourceTable
    {
        public ModuleFpTable()
           : base("MODULE_FP")
        {
            AddColumn(new BigIntColumn("module_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(new FixedCharColumn("fix_pref", 1, ColumnNullable.False));
            AddColumn(new IntColumn("preference"));
            AddColumn(new Ct7UniqueNameColumn("module_unique_name"));
            AddColumn(new Ct7NameColumn("module_name"));

            AddPrimaryKey("module_id", "resource_type", "resource_id", "fix_pref");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddModuleIdAndNameMapping(c);
            m.AddResourceIdAndNameMapping();
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
