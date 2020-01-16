namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ModuleClusterTable : V7StagingTable
    {
        public ModuleClusterTable(string schemaName)
           : base("CT_MODULE_CLUSTER", schemaName)
        {
            AddColumn(new BigIntColumn("module_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());

            AddColumnReferenceCheck(new ModuleIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
