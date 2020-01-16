namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Resources
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class LayoutTable : V7StagingTable
    {
        public LayoutTable(string schemaName)
           : base("CT_LAYOUT", schemaName)
        {
            AddColumn(new BigIntColumn("room_layout_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(new Ct7BoolColumn("complex"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
