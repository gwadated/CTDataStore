namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomTable : PublicResourceTable
    {
        public RoomTable()
           : base("ROOM")
        {
            AddColumn(new BigIntColumn("room_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(ColumnUtils.CreateSiteIdAndNameColumns());
            AddColumn(new FloatColumn("area"));
            AddColumn(ColumnUtils.CreateStaff1And2ColumnsWithNames());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(ColumnUtils.CreateSchedulableColumn());
            AddColumn(new IntColumn("default_capacity"));
            AddColumn(new FloatColumn("charge"));
            AddColumn(new Ct7TelephoneColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(ColumnUtils.CreateSpecialNeedsColumns());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("room_id");
            AddUniqueNameIndex();
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Room, "room_id");
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);
            m.AddSiteIdAndNameMapping(c);
            m.AddStaff1And2IdAndNameMapping(c);
            m.AddSchedulableMapping();
            m.AddSpecialNeedsMapping();

            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
