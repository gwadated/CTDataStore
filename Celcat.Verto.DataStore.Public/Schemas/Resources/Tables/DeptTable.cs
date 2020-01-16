namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class DeptTable : PublicResourceTable
    {
        public DeptTable()
           : base("DEPT")
        {
            AddColumn(new BigIntColumn("dept_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(new IntColumn("colour"));
            AddColumn(ColumnUtils.CreateStaff1And2ColumnsWithNames());
            AddColumn(new Ct7TelephoneColumn());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("dept_id");
            AddNameIndex();
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddConsolidatedOrFederatedIdMapping(c, Entity.Dept, "dept_id");
            m.AddFacultyIdAndNameMapping(c);
            m.AddStaff1And2IdAndNameMapping(c);
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
