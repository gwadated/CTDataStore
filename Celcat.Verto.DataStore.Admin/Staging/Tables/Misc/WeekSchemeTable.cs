namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Misc
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class WeekSchemeTable : V7StagingTable
    {
        public WeekSchemeTable(string schemaName)
           : base("CT_WEEK_SCHEME", schemaName)
        {
            AddColumn(new BigIntColumn("week_scheme_id"));
            AddColumn(new Ct7NameColumn());

            for (int n = 0; n < StagingSchema.MaxWeeksInTimetable; ++n)
            {
                string colName = string.Concat("week_number", n + 1);
                AddColumn(new StringColumn(colName, ColumnConstants.StrLenStd));
            }

            AddColumn(ColumnUtils.CreateAuditColumns());

            RegisterFederatedIdCols();
            RegisterConsolidatedIdCols();
        }
    }
}
