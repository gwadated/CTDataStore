namespace Celcat.Verto.DataStore.Public.Schemas.Attendance.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class MarkTable : PublicAttendanceTable
    {
        public MarkTable()
           : base("MARK")
        {
            AddColumn(new BigIntColumn("mark_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(new NotNullStringColumn("abbreviation"));
            AddColumn(new FixedNCharColumn("shortcut_key", 1, ColumnNullable.False));
            AddColumn(new IntColumn("color"));
            AddColumn(new FixedCharColumn("definition", 1, ColumnNullable.False));
            AddColumn(new NotNullStringColumn("definition_str"));
            AddColumn(new BitColumn("precedence"));
            AddColumn(new BitColumn("card"));
            AddColumn(new BitColumn("send_notification"));
            AddColumn(new StringColumn("notification_text", ColumnConstants.StrLenComments));
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("mark_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("mark_id");
            m.AddDescriptionMapping();
            m.AddMarkDefinition();
            m.AddOriginMapping();
            m.AddBooleanMapping("precedence");
            m.AddBooleanMapping("card");
            m.AddBooleanMapping("send_notification");

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
