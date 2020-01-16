namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamSlotTable : V7StagingTable
    {
        public ExamSlotTable(string schemaName)
           : base("CT_ES_SLOT", schemaName)
        {
            AddColumn(new BigIntColumn("slot_id"));
            AddColumn(new BigIntColumn("session_id"));
            AddColumn(new IntColumn("slot_day"));
            AddColumn(new DateTimeColumn("start_time"));
            AddColumn(new DateTimeColumn("end_time"));

            AddColumnReferenceCheck(new ExamSessionIdReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
