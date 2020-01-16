namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;
    
    internal class NotificationTable : PublicAttendanceTable
    {
        public NotificationTable()
           : base("NOTIFICATION")
        {
            AddColumn(new BigIntColumn("message_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("activity_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("msg_text", ColumnConstants.StrLenDescription));
            AddColumn(new BitColumn("sent"));
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(new Ct7UniqueNameColumn("student_unique_name"));
            AddColumn(new Ct7NameColumn("student_name"));

            AddPrimaryKey("message_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("message_id");
            m.AddStudentIdAndNameMapping(c);
            m.AddFederatedIdMapping("activity_id");
            m.AddBooleanMapping("sent");
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
