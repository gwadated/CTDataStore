namespace Celcat.Verto.DataStore.Admin.Staging.Tables.GroupMembership
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class FlatGroupSubGroupTable : V7StagingTable
    {
        public FlatGroupSubGroupTable(string schemaName)
           : base("CT_FLAT_GROUP_SUBGROUP", schemaName)
        {
            AddColumn(new BigIntColumn("group_id"));
            AddColumn(new BigIntColumn("subgroup_id"));
            AddColumn(new IntColumn("strength"));

            AddColumnReferenceCheck(new GroupIdReferenceCheck());
            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_GROUP", new[] { "group_id" }, new[] { "subgroup_id" }));

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
