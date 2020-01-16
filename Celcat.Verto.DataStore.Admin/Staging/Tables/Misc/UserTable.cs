namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Misc
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class UserTable : V7StagingTable
    {
        public UserTable(string schemaName)
           : base("CT_USER", schemaName)
        {
            AddColumn(new BigIntColumn("user_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(new BigIntColumn("student_id"));
            AddColumn(new Ct7BoolColumn("active"));
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7BoolColumn("booking_admin"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());

            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new StaffIdReferenceCheck());
            AddColumnReferenceCheck(new StudentIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
