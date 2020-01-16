namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RegisterMarkTable : PublicAttendanceTable
    {
        public RegisterMarkTable()
           : base("REGISTER_MARK")
        {
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("event_instance_id", ColumnConstants.StrLenEventInstance));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_week", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));
            AddColumn(new BigIntColumn("mark_id"));
            AddColumn(new Ct7NameColumn("mark_name"));
            AddColumn(new IntColumn("mins_late"));
            AddColumn(new StringColumn("comments", ColumnConstants.StrLenComments));
            AddColumn(new FixedCharColumn("source", 1));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());

            AddPrimaryKey("student_id", "event_instance_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddStudentIdAndNameMapping(c);
            m.AddEventInstanceMapping();
            m.AddFederatedIdMapping("event_id");
            m.AddSimpleMapping("timetable_week", "week");
            m.AddMarkIdAndNameMapping();
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
