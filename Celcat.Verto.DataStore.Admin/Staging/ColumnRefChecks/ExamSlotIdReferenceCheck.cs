namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class ExamSlotIdReferenceCheck : ColumnReferenceCheck
    {
        public ExamSlotIdReferenceCheck() 
            : base("CT_ES_SLOT", "slot_id")
        {
        }
    }
}
