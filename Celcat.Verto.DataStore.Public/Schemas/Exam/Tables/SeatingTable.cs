namespace Celcat.Verto.DataStore.Public.Schemas.Exam.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SeatingTable : PublicExamTable
    {
        public SeatingTable()
           : base("SEATING")
        {
            AddColumn(new BigIntColumn("exam_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("room_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("student_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new BigIntColumn("slot_id", ColumnNullable.False));
            AddColumn(new IntColumn("seat_index"));
            AddColumn(new NotNullStringColumn("exam_unique_name"));
            AddColumn(new NullStringColumn("exam_name"));
            AddColumn(new NotNullStringColumn("room_unique_name"));
            AddColumn(new NullStringColumn("room_name"));
            AddColumn(new NullStringColumn("room_layout_name"));
            AddColumn(new NotNullStringColumn("student_unique_name"));
            AddColumn(new NullStringColumn("student_name"));

            AddPrimaryKey("exam_id", "event_id", "room_id", "student_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddExamIdAndNameMapping();
            m.AddFederatedIdMapping("event_id");
            m.AddRoomIdAndNameMapping(c);
            m.AddStudentIdAndNameMapping(c);
            m.AddRoomLayoutIdAndNameMapping(c);
            m.AddFederatedIdMapping("slot_id");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
