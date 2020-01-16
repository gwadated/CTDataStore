namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class EventCatIdReferenceCheck : ColumnReferenceCheck
    {
        public EventCatIdReferenceCheck()
            : base("CT_EVENT_CAT", "event_cat_id")
        {
        }
    }
}
