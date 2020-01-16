namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class BookingIdReferenceCheck : ColumnReferenceCheck
    {
        public BookingIdReferenceCheck() 
            : base("CT_BOOKING", "booking_id")
        {
        }
    }
}
