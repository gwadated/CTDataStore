namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.TableBuilder.Columns;

    internal class CourseModuleTable : V7StagingTable
    {
        public CourseModuleTable(string schemaName)
           : base("CT_COURSE_MODULE", schemaName)
        {
            AddColumn(new BigIntColumn("course_id"));
            AddColumn(new BigIntColumn("module_id"));
            AddColumn(new FixedCharColumn("core_option", 1));

            AddColumnReferenceCheck(new CourseIdReferenceCheck());
            AddColumnReferenceCheck(new ModuleIdReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
