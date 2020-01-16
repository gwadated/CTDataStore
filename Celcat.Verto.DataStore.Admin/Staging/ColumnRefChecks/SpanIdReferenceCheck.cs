namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class SpanIdReferenceCheck : ColumnReferenceCheck
    {
        public SpanIdReferenceCheck() 
            : base("CT_SPAN", "span_id")
        {
        }
    }
}
