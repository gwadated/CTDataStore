namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class StaffIdReferenceCheck : ColumnReferenceCheck
    {
        public StaffIdReferenceCheck() 
            : base("CT_STAFF", "staff_id")
        {
        }
    }
}
