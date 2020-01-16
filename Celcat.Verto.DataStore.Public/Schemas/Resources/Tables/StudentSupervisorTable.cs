namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StudentSupervisorTable : PublicResourceTable
    {
        public StudentSupervisorTable()
           : base("STUDENT_SUPERVISOR")
        {
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("supervisor_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("student_unique_name"));
            AddColumn(new NullStringColumn("student_name"));
            AddColumn(new NotNullStringColumn("supervisor_name"));

            AddPrimaryKey("student_id", "supervisor_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddStudentIdAndNameMapping(c);
            m.AddSupervisorIdAndNameMapping(c);

            return m;
        }
    }
}
