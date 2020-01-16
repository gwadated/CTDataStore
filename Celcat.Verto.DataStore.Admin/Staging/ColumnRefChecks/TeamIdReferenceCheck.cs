namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class TeamIdReferenceCheck : ColumnReferenceCheck
    {
        public TeamIdReferenceCheck() 
            : base("CT_TEAM", "team_id")
        {
        }
    }
}
