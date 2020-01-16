namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomInventoryTable : PublicResourceTable
    {
        public RoomInventoryTable()
           : base("ROOM_INVENTORY")
        {
            AddColumn(new BigIntColumn("room_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("fixture_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("fixture_name"));
            AddColumn(new IntColumn("quantity"));
            AddColumn(new Ct7UniqueNameColumn("room_unique_name"));
            AddColumn(new Ct7NameColumn("room_name"));

            AddPrimaryKey("room_id", "fixture_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddRoomIdAndNameMapping(c);
            m.AddFixtureIdAndNameMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
