namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomLayoutTable : PublicResourceTable
    {
        public RoomLayoutTable()
           : base("ROOM_LAYOUT")
        {
            AddColumn(new BigIntColumn("room_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("room_layout_id", ColumnNullable.False));
            AddColumn(new IntColumn("capacity"));
            AddColumn(new Ct7UniqueNameColumn("room_unique_name"));
            AddColumn(new Ct7NameColumn("room_name"));
            AddColumn(new NotNullStringColumn("room_layout_name"));

            AddPrimaryKey("room_id", "room_layout_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddRoomIdAndNameMapping(c);
            m.AddRoomLayoutIdAndNameMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
