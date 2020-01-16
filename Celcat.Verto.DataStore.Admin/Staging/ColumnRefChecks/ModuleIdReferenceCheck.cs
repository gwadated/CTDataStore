namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class ModuleIdReferenceCheck : ColumnReferenceCheck
    {
        public ModuleIdReferenceCheck() 
            : base("CT_MODULE", "module_id")
        {
        }
    }
}
