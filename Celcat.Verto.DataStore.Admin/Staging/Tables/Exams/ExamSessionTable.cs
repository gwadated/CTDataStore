namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamSessionTable : V7StagingTable
    {
        public ExamSessionTable(string schemaName)
           : base("CT_ES_SESSION", schemaName)
        {
            AddColumn(new BigIntColumn("session_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new DateTimeColumn("start_date"));

            RegisterFederatedIdCols();
        }
    }
}
