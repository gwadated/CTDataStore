namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class WeekSchemeTable : PublicMiscTable
    {
        public WeekSchemeTable()
           : base("WEEK_SCHEME")
        {
            AddColumn(new BigIntColumn("week_scheme_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("name"));

            for (int n = 0; n < MiscSchema.MaxWeeksInTimetable; ++n)
            {
                var colName = string.Concat("week_number", n + 1);
                AddColumn(new StringColumn(colName, ColumnConstants.StrLenStd));
            }

            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());

            AddPrimaryKey("week_scheme_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddFederatedIdMapping("week_scheme_id");
            m.AddAuditMapping(c);

            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
