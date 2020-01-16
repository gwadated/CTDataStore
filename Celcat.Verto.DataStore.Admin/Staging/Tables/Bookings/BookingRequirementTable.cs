namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Bookings
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingRequirementTable : V7StagingTable
    {
        public BookingRequirementTable(string schemaName)
           : base("CT_BOOKING_REQUIREMENT", schemaName)
        {
            AddColumn(new BigIntColumn("booking_id"));
            AddColumn(ColumnUtils.CreateResourceTypeAndIdColumns());
            AddColumn(new Ct7BoolColumn("send_email"));

            AddColumnReferenceCheck(new BookingIdReferenceCheck());
            AddColumnReferenceCheck(new ResourceIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
