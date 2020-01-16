namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class WeekSpanTable : PublicMiscTable
    {
        public WeekSpanTable()
           : base("WEEK_SPAN")
        {
            AddColumn(new BigIntColumn("span_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("timetable_id", ColumnNullable.False));
            AddColumn(ColumnUtils.CreateUserIdAndNameColumns(ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));
            AddColumn(new Ct7WeeksColumn());
            AddColumn(new BitColumn("access"));

            AddPrimaryKey("span_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("span_id");
            m.AddSimpleMapping("timetable_id", "src_timetable_id");
            m.AddUserIdAndNameMapping(c);
            m.AddExplicitColumnMappingLookup("federated_span_id", "span_id", "name", Entity.Span);
            m.AddBooleanMapping("access");

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
