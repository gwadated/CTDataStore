namespace Celcat.Verto.DataStore.Public.Schemas.Booking.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingParticipantTable : PublicBookingTable
    {
        public BookingParticipantTable()
           : base("BOOKING_PARTICIPANT")
        {
            AddColumn(new BigIntColumn("booking_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new NullStringColumn("email"));
            AddColumn(new BitColumn("send_email", ColumnNullable.True));

            AddPrimaryKey("booking_id", "name");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("booking_id");
            m.AddBooleanMapping("send_email");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
