namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class ExamIdReferenceCheck : ColumnReferenceCheck
    {
        public ExamIdReferenceCheck() 
            : base("CT_ES_EXAM", "exam_id")
        {
        }
    }
}
