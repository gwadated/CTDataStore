namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupFpTable : V7StagingTable
    {
        public GroupFpTable(string schemaName)
           : base("CT_GROUP_FP", schemaName)
        {
            AddColumn(new BigIntColumn("group_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());
            AddColumn(new FixedCharColumn("fix_pref", 1));
            AddColumn(new IntColumn("preference"));

            AddColumnReferenceCheck(new GroupIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
