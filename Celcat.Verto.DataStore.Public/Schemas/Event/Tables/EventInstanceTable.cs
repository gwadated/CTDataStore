﻿namespace Celcat.Verto.DataStore.Public.Schemas.Event.Tables
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder.Columns;
    using Rhino.Etl.Core;

    internal class EventInstanceTable : PublicEventTable
    {
        public EventInstanceTable()
           : base("EVENT_INSTANCE")
        {
            AddColumn(new NotNullStringColumn("event_instance_id", ColumnConstants.StrLenEventInstance));
            AddColumn(new BigIntColumn("event_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("timetable_id", ColumnNullable.False));
            AddColumn(new BigIntColumn("timetable_event_id", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_week", ColumnNullable.False));
            AddColumn(new IntColumn("timetable_occurrence", ColumnNullable.False));
            AddColumn(new StringColumn("event_name", ColumnConstants.StrLenStd));
            AddColumn(new IntColumn("day_of_week", ColumnNullable.False));
            AddColumn(new DateTimeColumn("start_time", ColumnNullable.False));
            AddColumn(new DateTimeColumn("end_time", ColumnNullable.False));
            AddColumn(new IntColumn("break_mins"));
            AddColumn(ColumnUtils.CreateSpanIdAndNameColumns());
            AddColumn(ColumnUtils.CreateEventCatIdAndNameColumns());
            AddColumn(ColumnUtils.CreateCustomColumns());
            AddColumn(new IntColumn("capacity_req"));
            AddColumn(ColumnUtils.CreateDeptIdAndNameColumns());
            AddColumn(ColumnUtils.CreateFacultyIdAndNameColumns());
            AddColumn(new BitColumn("global_event"));
            AddColumn(new BitColumn("protected"));
            AddColumn(new BitColumn("suspended"));
            AddColumn(new IntColumn("grouping_id"));
            AddColumn(new BitColumn("registers_req", ColumnNullable.True));
            AddColumn(new BitColumn(ColumnConstants.RegistersReqResolvedColumnName));
            AddColumn(new Ct7NotesColumn());
            AddColumn(ColumnUtils.CreateLookupColumns());
            AddColumn(ColumnUtils.CreateAuditColumnsWithNames());
            AddColumn(ColumnUtils.CreateOriginColumns());

            AddPrimaryKey("event_instance_id");
        }

        public override TableColumnMappings GetColumnMappingsFromStage(DataStoreConfiguration c)
        {
            // establish the column mappings between the staging tables and this public table
            var m = new TableColumnMappings { EventExpansion = EventExpansionDefinition.Standard };

            m.AddSimpleMapping("timetable_id", "src_timetable_id");
            m.AddSimpleMapping("timetable_event_id", "event_id"); // must come before "event_id" mapping

            m.AddFederatedIdMapping("event_id");
            m.AddEventStartEndTimeMapping("start_time");
            m.AddEventStartEndTimeMapping("end_time");

            m.AddSpanIdAndNameMapping();
            m.AddEventCatIdAndNameMapping(c);
            m.AddDeptIdAndNameMapping(c);
            m.AddFacultyIdAndNameMapping(c);

            m.AddBooleanMapping("global_event");
            m.AddBooleanMapping("protected");
            m.AddBooleanMapping("suspended");
            m.AddBooleanMapping("registers_req");
            m.AddBooleanMapping(ColumnConstants.RegistersReqResolvedColumnName);

            m.AddAuditMapping(c);
            m.AddOriginMapping();

            m.AddRemainingSimpleMappings(Columns);

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
            var b = new SqlBuilder();

            long eventId = (long)stagingRow["event_id"];

            b.AppendFormat("delete from {0}", QualifiedName);
            b.Append("where event_instance_id like @P1");

            var parameters = new List<SqlParameter> { new SqlParameter("@P1", $"{eventId}-%") };
            DatabaseUtils.ExecuteSql(sqlConnectionString, b.ToString(), timeoutSecs, parameters.ToArray());
        }
    }
}
