namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class WeekSpanDateTable : PublicMiscTable
    {
        public WeekSpanDateTable()
           : base("WEEK_SPAN_DATE")
        {
            AddColumn(new BigIntColumn("span_id", ColumnNullable.False));
            AddColumn(new DateTimeColumn("span_date", ColumnNullable.False));
            AddColumn(new BigIntColumn("timetable_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("span_name"));
            AddColumn(new IntColumn("span_week_number", ColumnNullable.False));
            AddColumn(new IntColumn("span_week_occurrence", ColumnNullable.False));

            AddPrimaryKey("span_id", "span_date");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            var m = new TableColumnMappings { SpanExpansionRequired = true };

            m.AddFederatedIdMapping("span_id");
            m.AddSimpleMapping("timetable_id", "src_timetable_id");
            m.AddSimpleMapping("span_name", "name");

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
