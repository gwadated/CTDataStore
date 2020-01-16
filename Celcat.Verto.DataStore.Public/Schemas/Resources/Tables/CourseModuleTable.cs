namespace Celcat.Verto.DataStore.Public.Schemas.Resources.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class CourseModuleTable : PublicResourceTable
    {
        public CourseModuleTable()
           : base("COURSE_MODULE")
        {
            AddColumn(new BigIntColumn("course_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("module_id", ColumnNullable.False));
            AddColumn(new FixedCharColumn("core_option", 1));
            AddColumn(new NotNullStringColumn("course_name"));
            AddColumn(new NotNullStringColumn("module_unique_name"));
            AddColumn(new NullStringColumn("module_name"));

            AddPrimaryKey("course_id", "module_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddCourseIdAndNameMapping(c);
            m.AddModuleIdAndNameMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
