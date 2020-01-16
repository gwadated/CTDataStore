namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class AttendTable : PublicAttendanceTable
    {
        public AttendTable()
           : base("ATTEND")
        {
            AddColumn(new BigIntColumn("attend_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("activity_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));
            AddColumn(new BigIntColumn("mark_id"));
            AddColumn(new Ct7NameColumn("mark_name"));
            AddColumn(new IntColumn("mins_late"));
            AddColumn(new StringColumn("comments", ColumnConstants.StrLenComments));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());

            AddPrimaryKey("attend_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("attend_id");
            m.AddFederatedIdMapping("activity_id");
            m.AddStudentIdAndNameMapping(c);
            m.AddMarkIdAndNameMapping();
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
