namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class AuditReferenceCheck : ColumnReferenceCheck
    {
        public AuditReferenceCheck() 
            : base("CT_USER", new[] { "user_id" }, new[] { "user_id_change" })
        {
        }
    }
}
