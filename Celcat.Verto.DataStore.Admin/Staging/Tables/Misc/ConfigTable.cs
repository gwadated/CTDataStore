namespace Celcat.Verto.DataStore.Admin.Staging.Tables.Misc
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.TableBuilder.Columns;

    internal class ConfigTable : V7StagingTable
    {
        public ConfigTable(string schemaName)
           : base("CT_CONFIG", schemaName)
        {
            AddColumn(new StringColumn("timetable_name", ColumnConstants.StrLenStd));
            AddColumn(new IntColumn("version"));
            AddColumn(new IntColumn("no_weeks"));
            AddColumn(new IntColumn("no_periods"));
            AddColumn(new Ct7BoolColumn("registers_req"));

            for (int n = 0; n < StagingSchema.MaxWeeksInTimetable; ++n)
            {
                string colName = string.Concat("week", n + 1, "_date");
                AddColumn(new DateTimeColumn(colName));
            }

            for (int n = 0; n < StagingSchema.MaxPeriodsPerDayInTimetable; ++n)
            {
                AddColumn(new IntColumn(string.Concat("period", n + 1, "_start")));
                AddColumn(new IntColumn(string.Concat("period", n + 1, "_end")));
            }

            AddColumn(new GuidColumn("guid"));
        }
    }
}
