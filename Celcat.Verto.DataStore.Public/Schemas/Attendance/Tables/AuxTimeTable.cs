namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AuxTimeTable : PublicAttendanceTable
    {
        public AuxTimeTable()
           : base("AUX_TIME")
        {
            AddColumn(new BigIntColumn("activity_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames(ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));

            AddPrimaryKey("activity_id", "student_id", "resource_type", "resource_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("activity_id");
            m.AddStudentIdAndNameMapping(c);
            m.AddResourceIdAndNameMapping();
            m.AddAuditMapping(c);

            return m;
        }
    }
}
