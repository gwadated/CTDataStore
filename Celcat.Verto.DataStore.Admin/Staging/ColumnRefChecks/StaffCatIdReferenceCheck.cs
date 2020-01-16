namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class StaffCatIdReferenceCheck : ColumnReferenceCheck
    {
        public StaffCatIdReferenceCheck() 
            : base("CT_STAFF_CAT", "staff_cat_id")
        {
        }
    }
}
