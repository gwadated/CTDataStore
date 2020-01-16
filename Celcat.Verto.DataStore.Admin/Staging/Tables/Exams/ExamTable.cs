namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Exams
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ExamTable : V7StagingTable
    {
        public ExamTable(string schemaName)
           : base("CT_ES_EXAM", schemaName)
        {
            AddColumn(new BigIntColumn("exam_id"));
            AddColumn(new Ct7UniqueNameColumn());
            AddColumn(new Ct7NameColumn());
            AddColumn(new BigIntColumn("session_id"));
            AddColumn(new IntColumn("duration"));
            AddColumn(new BigIntColumn("event_cat_id"));
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("capacity_req"));
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new Ct7BoolColumn("protected"));
            AddColumn(new Ct7BoolColumn("suspended"));
            AddColumn(new IntColumn("grouping_id"));
            AddColumn(new Ct7BoolColumn("registers_req"));
            AddColumn(new Ct7BoolColumn(ColumnConstants.RegistersReqResolvedColumnName));
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new ExamSessionIdReferenceCheck());
            AddColumnReferenceCheck(new EventCatIdReferenceCheck());
            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
