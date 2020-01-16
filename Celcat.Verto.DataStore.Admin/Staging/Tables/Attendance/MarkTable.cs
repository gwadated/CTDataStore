namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Attendance
{
    using Celcat.Verto.DataStore.Admin.Staging.ColumnRefChecks;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class MarkTable : V7StagingTable
    {
        public MarkTable(string schemaName)
           : base("CT_AT_MARK", schemaName)
        {
            AddColumn(new BigIntColumn("mark_id"));
            AddColumn(new Ct7NameColumn());
            AddColumn(new Ct7DescriptionColumn());
            AddColumn(new StringColumn("abbreviation", ColumnConstants.StrLenStd));
            AddColumn(new FixedNCharColumn("shortcut_key", 1));
            AddColumn(new IntColumn("color"));
            AddColumn(new FixedCharColumn("definition", 1));
            AddColumn(new Ct7BoolColumn("precedence"));
            AddColumn(new Ct7BoolColumn("card"));
            AddColumn(new Ct7BoolColumn("send_notification"));
            AddColumn(new StringColumn("notification_text", ColumnConstants.StrLenComments));
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddColumnReferenceCheck(new OriginReferenceCheck());

            RegisterFederatedIdCols();
        }
    }
}
