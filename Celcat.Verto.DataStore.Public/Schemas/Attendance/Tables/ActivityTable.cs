namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ActivityTable : PublicAttendanceTable
    {
        public ActivityTable()
           : base("ACTIVITY")
        {
            AddColumn(new BigIntColumn("activity_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("event_instance_id", ColumnConstants.StrLenEventInstance));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_week", ColumnNullable.False));
            AddColumn(new DateTimeColumn("start_datetime", ColumnNullable.False));
            AddColumn(new DateTimeColumn("end_datetime", ColumnNullable.False));
            AddColumn(new BitColumn("closed"));
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateStaffIdAndNameColumns());
            AddColumn(new BitColumn("staff_present"));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("activity_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("activity_id");
            m.AddEventInstanceMapping();
            m.AddFederatedIdMapping("event_id");
            m.AddSimpleMapping("timetable_week", "week");
            m.AddBooleanMapping("closed");
            m.AddStaffIdAndNameMapping(c);
            m.AddBooleanMapping("staff_present");
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
