namespace Celcat.Verto.DataStore.Public.Schemas.Booking.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class BookingTable : PublicBookingTable
    {
        public BookingTable()
           : base("BOOKING")
        {
            AddColumn(new BigIntColumn("booking_id", ColumnNullable.False));
            AddColumn(new NullStringColumn("title"));
            AddColumn(ColumnUtils.CreateUserIdAndNameColumns());
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(new Ct7NotesColumn("search_criteria"));
            AddColumn(ColumnUtils.CreateEventCatIdAndNameColumns());
            AddColumn(new NullStringColumn("requester_name"));
            AddColumn(new NullStringColumn("requester_email"));
            AddColumn(new BitColumn("add_me"));
            AddColumn(new IntColumn("status", ColumnNullable.False));
            AddColumn(new Ct7NotesColumn());
            AddColumn(new Ct7NotesColumn("audit_notes"));
            AddColumn(new IntColumn("sb_status"));
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("booking_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("booking_id");
            m.AddUserIdAndNameMapping(c);
            m.AddDeptIdAndNameMapping(c);
            m.AddEventCatIdAndNameMapping(c);
            m.AddBooleanMapping("add_me");
            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
