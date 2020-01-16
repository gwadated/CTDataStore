namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class FixtureIdReferenceCheck : ColumnReferenceCheck
    {
        public FixtureIdReferenceCheck() 
            : base("CT_FIXTURE", "fixture_id")
        {
        }
    }
}
