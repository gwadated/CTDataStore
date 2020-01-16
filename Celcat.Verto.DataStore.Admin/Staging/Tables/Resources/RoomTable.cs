namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class RoomTable : V7StagingTable
    {
        public RoomTable(string schemaName)
           : base("CT_ROOM", schemaName)
        {
            AddColumn(new BigIntColumn("room_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new BigIntColumn("site_id"));
            AddColumn(new FloatColumn("area"));
            AddColumn(ColumnUtils.CreateStaff1And2Columns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new Ct7SchedulableColumn());
            AddColumn(new IntColumn("default_capacity"));
            AddColumn(new FloatColumn("charge"));
            AddColumn(new Ct7TelephoneColumn());
            AddColumn(new Ct7WebColumn());
            AddColumn(ColumnUtils.CreateSpecialNeedsColumns());
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new SiteIdReferenceCheck());
            AddColumnReferenceCheck(new StaffReferenceCheck(1));
            AddColumnReferenceCheck(new StaffReferenceCheck(2));
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
