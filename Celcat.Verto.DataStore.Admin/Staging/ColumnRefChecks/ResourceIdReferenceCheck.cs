namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class ResourceIdReferenceCheck : ColumnReferenceCheck
    {
        public ResourceIdReferenceCheck() 
            : base(string.Empty, "resource_type", "resource_id")
        {
        }
    }
}
