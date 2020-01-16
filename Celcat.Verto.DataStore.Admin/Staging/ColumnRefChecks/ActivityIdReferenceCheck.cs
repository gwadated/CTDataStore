namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;
    
    internal class ActivityIdReferenceCheck : ColumnReferenceCheck
    {
        public ActivityIdReferenceCheck() 
            : base("CT_AT_ACTIVITY", "activity_id")
        {
        }
    }
}
