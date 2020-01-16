namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Bookings
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingParticipantTable : V7StagingTable
    {
        public BookingParticipantTable(string schemaName)
           : base("CT_BOOKING_PARTICIPANT", schemaName)
        {
            AddColumn(new BigIntColumn("booking_id"));
            AddColumn(new StringColumn("name", ColumnConstants.StrLenStd));
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7BoolColumn("send_email"));

            AddColumnReferenceCheck(new BookingIdReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
