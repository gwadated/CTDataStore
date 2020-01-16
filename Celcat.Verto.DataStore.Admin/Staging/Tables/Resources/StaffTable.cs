namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class StaffTable : V7StagingTable
    {
        public StaffTable(string schemaName)
           : base("CT_STAFF", schemaName)
        {
            AddColumn(new BigIntColumn("staff_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new StringColumn("title", ColumnConstants.StrLenStaffStudentTitle));
            AddColumn(new Ct7SexColumn());
            AddColumn(ColumnUtils.CreateAddressColumns());
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("allowance_week"));
            AddColumn(new IntColumn("allowance_total"));
            AddColumn(ColumnUtils.CreateTargetColumns());
            AddColumn(new Ct7SchedulableColumn());
            AddColumn(ColumnUtils.CreateStdTelColumns());
            AddColumn(new Ct7EmailColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(new StringColumn("profile", ColumnConstants.StrLenStd));
            AddColumn(ColumnUtils.CreateSpecialNeedsColumns());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new RoomIdReferenceCheck());
            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
