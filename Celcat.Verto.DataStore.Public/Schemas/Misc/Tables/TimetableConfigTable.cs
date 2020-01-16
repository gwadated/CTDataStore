namespace Celcat.Verto.DataStore.Public.Schemas.Misc.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;

    internal class TimetableConfigTable : PublicMiscTable
    {
        public TimetableConfigTable()
           : base("TIMETABLE_CONFIG")
        {
            AddColumn(new BigIntColumn("timetable_id", ColumnNullable.False));
            AddColumn(new NotNullStringColumn("timetable_name"));
            AddColumn(new IntColumn("version", ColumnNullable.False));
            AddColumn(new IntColumn("no_weeks", ColumnNullable.False));
            AddColumn(new IntColumn("no_periods", ColumnNullable.False));
            AddColumn(new BitColumn("registers_req"));

            for (int n = 0; n < MiscSchema.MaxWeeksInTimetable; ++n)
            {
                string colName = string.Concat("week", n + 1, "_date");
                AddColumn(new DateTimeColumn(colName));
            }

            for (int n = 0; n < MiscSchema.MaxPeriodsPerDayInTimetable; ++n)
            {
                AddColumn(new IntColumn(string.Concat("period", n + 1, "_start")));
                AddColumn(new IntColumn(string.Concat("period", n + 1, "_end")));
            }

            AddColumn(new GuidColumn("guid", ColumnNullable.False));

            AddPrimaryKey("timetable_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings();
            m.AddSimpleMapping("timetable_id", "src_timetable_id");
            m.AddBooleanMapping("registers_req");
            m.AddRemainingSimpleMappings(Columns);

            return m;
        }
    }
}
