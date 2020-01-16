namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffTable : PublicResourceTable
    {
        public StaffTable()
           : base("STAFF")
        {
            AddColumn(new BigIntColumn("staff_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("title", ColumnConstants.StrLenStaffStudentTitle));
            AddColumn(new Ct7SexColumn());
            AddColumn(ColumnUtils.CreateAddressColumns());
            AddColumn(ColumnUtils.CreateRoomIdAndNameColumns());
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("allowance_week"));
            AddColumn(new IntColumn("allowance_total"));
            AddColumn(ColumnUtils.CreateTargetColumns());
            AddColumn(ColumnUtils.CreateSchedulableColumn());
            AddColumn(ColumnUtils.CreateStdTelColumns());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(new StringColumn("profile", ColumnConstants.StrLenStd));
            AddColumn(ColumnUtils.CreateSpecialNeedsColumns());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("staff_id");
            AddUniqueNameIndex();
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Staff, "staff_id");
            m.AddRoomIdAndNameMapping(c);
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);
            m.AddSchedulableMapping();
            m.AddSpecialNeedsMapping();

            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
