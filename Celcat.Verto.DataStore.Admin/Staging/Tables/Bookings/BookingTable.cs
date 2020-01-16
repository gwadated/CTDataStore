namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Bookings
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingTable : V7StagingTable
    {
        public BookingTable(string schemaName)
           : base("CT_BOOKING", schemaName)
        {
            AddColumn(new BigIntColumn("booking_id"));
            AddColumn(new StringColumn("title", ColumnConstants.StrLenStd));
            AddColumn(new BigIntColumn("user_id"));
            AddColumn(new BigIntColumn("dept_id"));
            AddColumn(new Ct7NotesColumn("search_criteria"));
            AddColumn(new BigIntColumn("event_cat_id"));
            AddColumn(new StringColumn("requester_name", ColumnConstants.StrLenStd));
            AddColumn(new StringColumn("requester_email", ColumnConstants.StrLenStd));
            AddColumn(new Ct7BoolColumn("add_me"));
            AddColumn(new IntColumn("status"));
            AddColumn(new Ct7NotesColumn());
            AddColumn(new Ct7NotesColumn("audit_notes"));
            AddColumn(new IntColumn("sb_status"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumns());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new ColumnReferenceCheck("CT_USER", "user_id"));
            AddColumnReferenceCheck(new DeptIdReferenceCheck());
            AddColumnReferenceCheck(new EventCatIdReferenceCheck());
            AddColumnReferenceCheck(new AuditReferenceCheck());
            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
