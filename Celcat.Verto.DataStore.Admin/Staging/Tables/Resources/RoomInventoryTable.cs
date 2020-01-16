namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomInventoryTable : V7StagingTable
    {
        public RoomInventoryTable(string schemaName)
           : base("CT_ROOM_INVENTORY", schemaName)
        {
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new BigIntColumn("fixture_id"));
            AddColumn(new IntColumn("quantity"));

            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new FixtureIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
