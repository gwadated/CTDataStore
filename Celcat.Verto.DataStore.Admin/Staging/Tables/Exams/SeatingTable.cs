namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class SeatingTable : V7StagingTable
    {
        public SeatingTable(string schemaName)
           : base("CT_ES_SEATING", schemaName)
        {
            AddColumn(new BigIntColumn("exam_id"));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new BigIntColumn("slot_id"));
            AddColumn(new IntColumn("seat_index"));

            AddColumnReferenceCheck(new ExamIdReferenceCheck());
            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new RoomLayoutIdReferenceCheck());
            AddColumnReferenceCheck(new ExamSlotIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
