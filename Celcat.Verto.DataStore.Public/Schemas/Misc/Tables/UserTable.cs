namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class UserTable : PublicMiscTable
    {
        public UserTable()
           : base("TIMETABLE_USER")
        {
            AddColumn(new BigIntColumn("user_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(ColumnUtils.CreateStaffIdAndNameColumns());
            AddColumn(ColumnUtils.CreateStudentIdAndNameColumns());
            AddColumn(new BitColumn("active"));
            AddColumn(new Ct7EmailColumn());
            AddColumn(new BitColumn("booking_admin"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());

            AddPrimaryKey("user_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.User, "user_id");
            m.AddColumnMappingLookup("user_id", "name", Entity.User, c);
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);
            m.AddStaffIdAndNameMapping(c);
            m.AddStudentIdAndNameMapping(c);
            m.AddBooleanMapping("active");
            m.AddBooleanMapping("booking_admin");
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
