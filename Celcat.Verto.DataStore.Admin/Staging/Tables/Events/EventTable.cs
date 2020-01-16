namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Events
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class EventTable : V7StagingTable
    {
        public EventTable(string schemaName)
           : base("CT_EVENT", schemaName)
        {
            AddColumn(new BigIntColumn("event_id"));
            AddColumn(new StringColumn("event_name", ColumnConstants.StrLenStd));
            AddColumn(new IntColumn("day_of_week"));
            AddColumn(new DateTimeColumn("start_time"));
            AddColumn(new DateTimeColumn("end_time"));
            AddColumn(new IntColumn("break_mins"));
            AddColumn(new Ct7WeeksColumn());
            AddColumn(new BigIntColumn("span_id"));
            AddColumn(new BigIntColumn("event_cat_id"));
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("capacity_req"));
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new Ct7BoolColumn("global_event"));
            AddColumn(new Ct7BoolColumn("protected"));
            AddColumn(new Ct7BoolColumn("suspended"));
            AddColumn(new IntColumn("grouping_id"));
            AddColumn(new Ct7BoolColumn("registers_req"));
            AddColumn(new Ct7BoolColumn(ColumnConstants.RegistersReqResolvedColumnName));
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new SpanIdReferenceCheck());
            AddColumnReferenceCheck(new EventCatIdReferenceCheck());
            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
