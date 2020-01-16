namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class GroupTable : V7StagingTable
    {
        public GroupTable(string schemaName)
           : base("CT_GROUP", schemaName)
        {
            AddColumn(new BigIntColumn("group_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7AcademicYearColumn());
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(ColumnUtils.CreateStaff1And2Columns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(ColumnUtils.CreateTargetColumns());
            AddColumn(new IntColumn("target_size"));
            AddColumn(new IntColumn("group_size"));
            AddColumn(new IntColumn("additional"));
            AddColumn(new IntColumn("split_id"));
            AddColumn(new Ct7SchedulableColumn());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new StaffReferenceCheck(1));
            AddColumnReferenceCheck(new StaffReferenceCheck(2));
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
