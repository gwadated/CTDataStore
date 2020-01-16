namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;
 
    internal class SupervisorIdReferenceCheck : ColumnReferenceCheck
    {
        public SupervisorIdReferenceCheck() 
            : base("CT_SUPERVISOR", "supervisor_id")
        {
        }
    }
}
