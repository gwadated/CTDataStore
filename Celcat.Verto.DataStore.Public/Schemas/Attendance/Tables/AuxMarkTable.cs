namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AuxMarkTable : PublicAttendanceTable
    {
        public AuxMarkTable()
           : base("AUX_MARK")
        {
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames(ColumnNullable.False));
            AddColumn(ColumnUtils.CreateCustomColumns(2));
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));

            AddPrimaryKey("student_id", "resource_type", "resource_id", "date_change");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddStudentIdAndNameMapping(c);
            m.AddResourceIdAndNameMapping();
            m.AddAuditMapping(c);
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
