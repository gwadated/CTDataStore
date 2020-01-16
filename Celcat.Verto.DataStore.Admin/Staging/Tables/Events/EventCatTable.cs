namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventCatTable : V7StagingTable
    {
        public EventCatTable(string schemaName)
           : base("CT_EVENT_CAT", schemaName)
        {
            AddColumn(new BigIntColumn("event_cat_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(new IntColumn("colour"));
            AddColumn(new FloatColumn("weighting"));
            AddColumn(new Ct7BoolColumn("registers_req"));
            AddColumn(new Ct7BoolColumn(ColumnConstants.RegistersReqResolvedColumnName));
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
