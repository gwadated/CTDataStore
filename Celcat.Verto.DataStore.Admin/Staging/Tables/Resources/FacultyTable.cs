namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class FacultyTable : V7StagingTable
    {
        public FacultyTable(string schemaName)
           : base("CT_FACULTY", schemaName)
        {
            AddColumn(new BigIntColumn("faculty_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new IntColumn("colour"));
            AddColumn(ColumnUtils.CreateStaff1And2Columns());
            AddColumn(new Ct7TelephoneColumn());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new StaffReferenceCheck(1));
            AddColumnReferenceCheck(new StaffReferenceCheck(2));
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
