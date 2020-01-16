namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class SiteIdReferenceCheck : ColumnReferenceCheck
    {
        public SiteIdReferenceCheck() 
            : base("CT_SITE", "site_id")
        {
        }
    }
}
