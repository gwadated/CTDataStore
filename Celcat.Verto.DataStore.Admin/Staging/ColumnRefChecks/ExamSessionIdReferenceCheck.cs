namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class ExamSessionIdReferenceCheck : ColumnReferenceCheck
    {
        public ExamSessionIdReferenceCheck() 
            : base("CT_ES_SESSION", "session_id")
        {
        }
    }
}
