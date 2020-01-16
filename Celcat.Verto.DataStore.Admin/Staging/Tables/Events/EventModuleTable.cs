namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventModuleTable : V7StagingTable
    {
        public EventModuleTable(string schemaName)
           : base("CT_EVENT_MODULE", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("module_id"));
            AddColumn(new Ct7WeeksColumn());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new ModuleIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
