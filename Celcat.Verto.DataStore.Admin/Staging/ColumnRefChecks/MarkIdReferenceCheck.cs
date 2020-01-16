namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;
    
    internal class MarkIdReferenceCheck : ColumnReferenceCheck
    {
        public MarkIdReferenceCheck() 
            : base("CT_AT_MARK", "mark_id")
        {
        }
    }
}
