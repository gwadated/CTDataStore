namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventFixtureReqTable : V7StagingTable
    {
        public EventFixtureReqTable(string schemaName)
           : base("CT_EVENT_FIXTURE_REQ", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("fixture_id"));
            AddColumn(new IntColumn("quantity"));

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new FixtureIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
