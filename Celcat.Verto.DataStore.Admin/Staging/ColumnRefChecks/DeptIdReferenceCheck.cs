namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class DeptIdReferenceCheck : ColumnReferenceCheck
    {
        public DeptIdReferenceCheck() 
            : base("CT_DEPT", "dept_id")
        {
        }
    }
}
