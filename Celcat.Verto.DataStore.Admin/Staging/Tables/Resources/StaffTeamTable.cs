namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffTeamTable : V7StagingTable
    {
        public StaffTeamTable(string schemaName)
           : base("CT_STAFF_TEAM", schemaName)
        {
            AddColumn(new BigIntColumn("team_id"));
            AddColumn(new BigIntColumn("staff_id"));

            AddColumnReferenceCheck(new TeamIdReferenceCheck());
            AddColumnReferenceCheck(new StaffIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
