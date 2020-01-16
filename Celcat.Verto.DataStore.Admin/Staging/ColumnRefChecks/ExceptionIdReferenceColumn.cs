namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;
    
    internal class ExceptionIdReferenceColumn : ColumnReferenceCheck
    {
        public ExceptionIdReferenceColumn() 
            : base("CT_AT_EXCEPTION", "exception_id")
        {
        }
    }
}
