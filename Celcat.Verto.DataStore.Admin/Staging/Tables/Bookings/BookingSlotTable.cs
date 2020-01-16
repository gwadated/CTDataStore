namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Bookings
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingSlotTable : V7StagingTable
    {
        public BookingSlotTable(string schemaName)
           : base("CT_BOOKING_SLOT", schemaName)
        {
            AddColumn(new BigIntColumn("booking_id"));
            AddColumn(new BigIntColumn("booking_slot_id"));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new DateTimeColumn("start_dt"));
            AddColumn(new DateTimeColumn("end_dt"));
            AddColumn(new IntColumn("break_mins"));
            
            AddColumnReferenceCheck(new BookingIdReferenceCheck());
            AddColumnReferenceCheck(new EventIdReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
