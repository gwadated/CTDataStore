namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class AttendTimeTable : PublicAttendanceTable
    {
        public AttendTimeTable()
           : base("ATTEND_TIME")
        {
            AddColumn(new BigIntColumn("attend_time_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("activity_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));
            AddColumn(new DateTimeColumn("in_time", ColumnNullable.False));
            AddColumn(new DateTimeColumn("out_time", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());

            AddPrimaryKey("attend_time_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("attend_time_id");
            m.AddFederatedIdMapping("activity_id");
            m.AddStudentIdAndNameMapping(c);
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
