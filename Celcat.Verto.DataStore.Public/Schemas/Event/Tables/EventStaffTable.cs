namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;
    using Rhino.Etl.Core;

    internal class EventStaffTable : PublicEventTable
    {
        public EventStaffTable()
           : base("EVENT_STAFF")
        {
            AddColumn(new NotNullStringColumn("event_instance_id", ColumnConstants.StrLenEventInstance));
            AddColumn(new BigIntColumn("staff_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("staff_cat_id"));
            AddColumn(new DateTimeColumn("start_time", ColumnNullable.False));
            AddColumn(new DateTimeColumn("end_time", ColumnNullable.False));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_week", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_occurrence", ColumnNullable.False));
            AddColumn(new Ct7UniqueNameColumn("staff_unique_name"));
            AddColumn(new Ct7NameColumn("staff_name"));
            AddColumn(new Ct7NameColumn("staff_cat_name"));

            AddPrimaryKey("event_instance_id", "staff_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table

            var m = new TableColumnMappings { EventExpansion = EventExpansionDefinition.Standard };

            m.AddStaffIdAndNameMapping(c);
            m.AddStaffCatIdAndNameMapping(c);
            m.AddEventStartEndTimeMapping("start_time");
            m.AddEventStartEndTimeMapping("end_time");
            m.AddFederatedIdMapping("event_id");

            return m;
        }

        public override void Delete(
            string sqlConnectionString, 
            int timeoutSecs, 
            Row stagingRow, 
            TableColumnMappings colMappings,
            FixupCaches caches, 
            DataStoreConfiguration configuration)
        {
            DeleteEventAssignment(sqlConnectionString, timeoutSecs, stagingRow);
        }
    }
}
