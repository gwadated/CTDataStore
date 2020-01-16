namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class StaffReferenceCheck : ColumnReferenceCheck
    {
        public StaffReferenceCheck(int num) 
            : base("CT_STAFF", new[] { "staff_id" }, new[] { string.Format("staff_id{0}", num) })
        {
        }
    }
}
