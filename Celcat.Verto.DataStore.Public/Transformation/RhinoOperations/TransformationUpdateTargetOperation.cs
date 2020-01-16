namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationUpdateTargetOperation : AbstractOperation
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly PublicTable _targetTable;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;
        private readonly TransformationType _transformationType;
        
        public TransformationUpdateTargetOperation(
            string connectionString, 
            int timeoutSecs,
            PublicTable targetTable, 
            FixupCaches caches, 
            DataStoreConfiguration configuration,
            TransformationType transformationType)
        {
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _targetTable = targetTable;
            _caches = caches;
            _configuration = configuration;
            _transformationType = transformationType;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            var colMappings = _targetTable.GetColumnMappingsFromStage(_configuration);
            if (colMappings != null)
            {
                if (colMappings.CountIncludingEventExpansion != _targetTable.Columns.Count)
                {
                    throw new ApplicationException($"Unexpected column count - table: {_targetTable.Name}");
                }

                switch (_transformationType)
                {
                    case TransformationType.Upsert:
                        return ExecuteUpsert(rows, colMappings);

                    case TransformationType.Delete:
                    default:
                        return ExecuteDelete(rows, colMappings);
                }
            }

            return rows;
        }

        private IEnumerable<Row> ExecuteDelete(IEnumerable<Row> rows, TableColumnMappings colMappings)
        {
            foreach (var r in rows)
            {
                _targetTable.Delete(_connectionString, _timeoutSecs, r, colMappings, _caches, _configuration);
                yield return r;
            }
        }
        
        private IEnumerable<Row> ExecuteUpsert(IEnumerable<Row> rows, TableColumnMappings colMappings)
        {
            var rowArray = rows.ToArray();

            bool eventExpand = colMappings.EventExpansionRequired;
            bool spanExpand = colMappings.SpanExpansionRequired;

            if (eventExpand)
            {
                SpecialEventExpansion(rowArray, colMappings);
            }
            else if (spanExpand)
            {
                SpecialSpanExpansion(rowArray, colMappings);
            }
            else
            {
                if (rowArray.Any())
                {
                    if (rowArray.Length > 10)
                    {
                        _targetTable.Upsert(_connectionString, _timeoutSecs, rowArray, colMappings, _caches, _configuration, modifyWeekNumbers: true);
                    }
                    else
                    {
                        // small number of rows so do upsert for each rather than go to the expense of packaging a bulk copy...
                        foreach (var r in rowArray)
                        {
                            _targetTable.Upsert(_connectionString, _timeoutSecs, r, colMappings, _caches, _configuration);
                        }
                    }
                }
            }

            yield break;
        }

        private void ConvertToOneBasedWeekNos(Row[] rows)
        {
            if (rows.Any())
            {
                var row1 = rows.FirstOrDefault();
                if (row1 != null)
                {
                    var cols = row1.Columns.ToArray();
                    if (cols.Any())
                    {
                        string wkCol = _targetTable.Name.Equals("REGISTER_MARK") || _targetTable.Name.Equals("ACTIVITY")
                           ? "week"
                           : "timetable_week";

                        if (cols.Contains(wkCol))
                        {
                            foreach (var r in rows)
                            {
                                r[wkCol] = (int)r[wkCol] + 1;
                            }
                        }
                    }
                }
            }
        }

        private void SpecialSpanExpansion(Row[] rows, TableColumnMappings colMappings)
        {
            var rowsToInsert = new List<Row>();
            var spanIds = new List<long>();

            foreach (var r in rows)
            {
                long spanId = (long)r["federated_span_id"];
                spanIds.Add(spanId);

                string wks = ((string)r["weeks"]).ToUpper();

                int timetableId = (int)r["src_timetable_id"];
                var weekDates = _caches.WeekDatesCache.Get(timetableId);

                int weekNum = 0;
                int occurrence = 0;

                foreach (var ch in wks)
                {
                    if (ch == 'Y')
                    {
                        ++occurrence;

                        var dt = weekDates.StartingDates[weekNum];
                        for (int day = 0; day < 7; ++day)
                        {
                            Row vr = r.Clone();
                            vr["span_date"] = dt.AddDays(day);
                            vr["span_week_number"] = weekNum + 1;
                            vr["span_week_occurrence"] = occurrence;

                            rowsToInsert.Add(vr);
                        }
                    }

                    ++weekNum;
                }
            }

            if (spanIds.Any())
            {
                // first remove all instances of span...
                _targetTable.DeleteSpanInstances(_connectionString, _timeoutSecs, spanIds);

                // then insert afresh...
                _targetTable.Upsert(_connectionString, _timeoutSecs, rowsToInsert, colMappings, _caches, _configuration);
            }
        }

        private void SpecialEventExpansion(Row[] rows, TableColumnMappings colMappings)
        {
            var rowsToInsert = new List<Row>();
            var eventIds = new List<long>();

            foreach (var r in rows)
            {
                long federatedEventId = (long)r[colMappings.EventExpansion.StagingFederatedEventIdColumn];
                eventIds.Add(federatedEventId);

                string wks = ((string)r[colMappings.EventExpansion.StagingWeeksColumn]).ToUpper();
                if (string.IsNullOrEmpty(wks))
                {
                    throw new ApplicationException("weeks are null!");
                }

                int weekNum = 0;
                int occurrence = 1;
                foreach (var ch in wks)
                {
                    if (ch == 'Y')
                    {
                        Row vr = r.Clone();
                        vr[colMappings.EventExpansion.PublicEventInstanceColumn] =
                           ColumnMappingLookup.ColumnMappingEventInstance.FabricateEventInstanceId(federatedEventId, weekNum);

                        vr[colMappings.EventExpansion.PublicWeekColumn] = weekNum + 1;
                        vr[colMappings.EventExpansion.PublicWeekOccurrenceColumn] = occurrence++;
                        rowsToInsert.Add(vr);
                    }

                    ++weekNum;
                }
            }

            if (eventIds.Any())
            {
                // first remove all instances of event...
                _targetTable.DeleteEventInstances(_connectionString, _timeoutSecs, eventIds);

                // then insert afresh...
                _targetTable.Upsert(_connectionString, _timeoutSecs, rowsToInsert, colMappings, _caches, _configuration);
            }
        }
    }
}
