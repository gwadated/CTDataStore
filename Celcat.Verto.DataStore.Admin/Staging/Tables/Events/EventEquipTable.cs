namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventEquipTable : V7StagingTable
    {
        public EventEquipTable(string schemaName)
           : base("CT_EVENT_EQUIP", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("equip_id"));
            AddColumn(new Ct7WeeksColumn());

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new EquipIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
