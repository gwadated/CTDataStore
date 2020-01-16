namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ModuleTable : PublicResourceTable
    {
        public ModuleTable()
           : base("MODULE")
        {
            AddColumn(new BigIntColumn("module_id", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7AcademicYearColumn());
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(ColumnUtils.CreateStaff1And2ColumnsWithNames());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(ColumnUtils.CreateTargetColumns());
            AddColumn(ColumnUtils.CreateSchedulableColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("module_id");
            AddUniqueNameIndex();
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Module, "module_id");
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);
            m.AddStaff1And2IdAndNameMapping(c);
            m.AddSchedulableMapping();
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
