namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ModuleFpTable : V7StagingTable
    {
        public ModuleFpTable(string schemaName)
           : base("CT_MODULE_FP", schemaName)
        {
            AddColumn(new BigIntColumn("module_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());
            AddColumn(new FixedCharColumn("fix_pref", 1));
            AddColumn(new IntColumn("preference"));

            AddColumnReferenceCheck(new ModuleIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
