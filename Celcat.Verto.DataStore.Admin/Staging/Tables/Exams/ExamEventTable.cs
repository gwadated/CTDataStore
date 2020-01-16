namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamEventTable : V7StagingTable
    {
        public ExamEventTable(string schemaName)
           : base("CT_ES_EXAM_EVENT", schemaName)
        {
            AddColumn(new BigIntColumn("exam_id"));
            AddColumn(new BigIntColumn("event_id"));

            AddColumnReferenceCheck(new ExamIdReferenceCheck());
            AddColumnReferenceCheck(new EventIdReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
