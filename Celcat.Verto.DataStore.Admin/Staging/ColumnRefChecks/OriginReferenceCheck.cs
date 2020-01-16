namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class OriginReferenceCheck : ColumnReferenceCheck
    {
        public OriginReferenceCheck() 
            : base("CT_ORIGIN", "origin_id")
        {
        }
    }
}
