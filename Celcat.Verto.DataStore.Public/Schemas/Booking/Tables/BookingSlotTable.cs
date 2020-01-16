namespace Celcat.Verto.DataStore.Public.Schemas.Booking.Tables
{
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingSlotTable : PublicBookingTable
    {
        public BookingSlotTable()
           : base("BOOKING_SLOT")
        {
            AddColumn(new BigIntColumn("booking_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("booking_slot_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new DateTimeColumn("start_dt", ColumnNullable.False));
            AddColumn(new DateTimeColumn("end_dt", ColumnNullable.False));
            AddColumn(new IntColumn("break_mins"));

            AddPrimaryKey("booking_id", "booking_slot_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("booking_id");
            m.AddFederatedIdMapping("event_id");

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
