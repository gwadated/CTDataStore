namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomLayoutTable : V7StagingTable
    {
        public RoomLayoutTable(string schemaName)
           : base("CT_ROOM_LAYOUT", schemaName)
        {
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new IntColumn("capacity"));

            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new RoomLayoutIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
