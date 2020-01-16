namespace Celcat.Verto.DataStore.Admin.Staging.Tables.GroupMembership
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupStudentTable : V7StagingTable
    {
        public GroupStudentTable(string schemaName)
           : base("CT_GROUP_STUDENT", schemaName)
        {
            AddColumn(new BigIntColumn("group_id"));
            AddColumn(new BigIntColumn("student_id"));

            AddColumnReferenceCheck(new GroupIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
