namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ModuleClusterTable : PublicResourceTable
    {
        public ModuleClusterTable()
           : base("MODULE_CLUSTER")
        {
            AddColumn(new BigIntColumn("module_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("module_unique_name"));
            AddColumn(new Ct7NameColumn("module_name"));

            AddPrimaryKey("module_id", "resource_type", "resource_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddModuleIdAndNameMapping(c);
            m.AddResourceIdAndNameMapping();

            return m;
        }
    }
}
