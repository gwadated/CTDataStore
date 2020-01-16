namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class InvigilationTable : V7StagingTable
    {
        public InvigilationTable(string schemaName)
           : base("CT_ES_INVIGILATION", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new BigIntColumn("slot_id"));

            AddColumnReferenceCheck(new EventIdReferenceCheck());
            AddColumnReferenceCheck(new ExamSlotIdReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
