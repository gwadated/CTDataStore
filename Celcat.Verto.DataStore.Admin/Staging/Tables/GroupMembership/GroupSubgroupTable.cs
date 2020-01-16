namespace Celcat.Verto.DataStore.Admin.Staging.Tables.GroupMembership
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupSubgroupTable : V7StagingTable
    {
        public GroupSubgroupTable(string schemaName)
           : base("CT_GROUP_SUBGROUP", schemaName)
        {
            AddColumn(new BigIntColumn("group_id"));
            AddColumn(new BigIntColumn("subgroup_id"));

            AddColumnReferenceCheck(new GroupIdReferenceCheck());
            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_GROUP", new[] { "group_id" }, new[] { "subgroup_id" }));

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
