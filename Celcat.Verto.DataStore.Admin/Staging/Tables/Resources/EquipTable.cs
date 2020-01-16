namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EquipTable : V7StagingTable
    {
        public EquipTable(string schemaName)
           : base("CT_EQUIP", schemaName)
        {
            AddColumn(new BigIntColumn("equip_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(ColumnUtils.CreateStaff1And2Columns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("booking_interval"));
            AddColumn(new Ct7SchedulableColumn());
            AddColumn(new FloatColumn("charge"));
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
