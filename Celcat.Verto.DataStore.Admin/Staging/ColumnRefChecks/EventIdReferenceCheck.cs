namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class EventIdReferenceCheck : ColumnReferenceCheck
    {
        public EventIdReferenceCheck() 
            : base("CT_EVENT", "event_id")
        {
        }
    }
}
