namespace Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks
{
    using Celcat.Verto.TableBuilder;

    internal class RoomIdReferenceCheck : ColumnReferenceCheck
    {
        public RoomIdReferenceCheck() 
            : base("CT_ROOM", "room_id")
        {
        }
    }
}
