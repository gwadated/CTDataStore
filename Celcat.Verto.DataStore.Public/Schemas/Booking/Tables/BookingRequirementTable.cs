namespace Celcat.Verto.DataStore.Public.Schemas.Booking.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingRequirementTable : PublicBookingTable
    {
        public BookingRequirementTable()
           : base("BOOKING_REQUIREMENT")
        {
            AddColumn(new BigIntColumn("booking_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumnsWithName(ColumnNullable.False));
            AddColumn(new BitColumn("send_email", ColumnNullable.True));

            AddPrimaryKey("booking_id", "resource_type", "resource_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("booking_id");
            m.AddResourceIdAndNameMapping();
            m.AddBooleanMapping("send_email");

            return m;
        }
    }
}
