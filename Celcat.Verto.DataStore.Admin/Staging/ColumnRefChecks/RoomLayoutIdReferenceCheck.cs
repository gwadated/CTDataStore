namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class RoomLayoutIdReferenceCheck : ColumnReferenceCheck
    {
        public RoomLayoutIdReferenceCheck() 
            : base("CT_LAYOUT", "room_layout_id")
        {
        }
    }
}
