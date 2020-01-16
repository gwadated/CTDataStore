namespace Celcat.Verto.DataStore.Public.Schemas.Membership.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupStudentTable : PublicMembershipTable
    {
        public GroupStudentTable()
           : base("GROUP_STUDENT")
        {
            AddColumn(new BigIntColumn("group_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("group_unique_name"));
            AddColumn(new NullStringColumn("group_name"));
            AddColumn(new NotNullStringColumn("student_unique_name"));
            AddColumn(new NullStringColumn("student_name"));

            AddPrimaryKey("group_id", "student_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddGroupIdAndNameMapping(c);
            m.AddStudentIdAndNameMapping(c);

            return m;
        }
    }
}
