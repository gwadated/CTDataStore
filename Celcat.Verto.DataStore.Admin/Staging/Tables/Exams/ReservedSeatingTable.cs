namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ReservedSeatingTable : V7StagingTable
    {
        public ReservedSeatingTable(string schemaName)
           : base("CT_ES_RESERVED_SEATING", schemaName)
        {
            AddColumn(new BigIntColumn("exam_id"));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new IntColumn("reservation_index"));
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new BigIntColumn("slot_id"));
            AddColumn(new IntColumn("seat_index"));

            AddColumnReferenceCheck(new ExamIdReferenceCheck());
            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new RoomLayoutIdReferenceCheck());
            AddColumnReferenceCheck(new ExamSlotIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
