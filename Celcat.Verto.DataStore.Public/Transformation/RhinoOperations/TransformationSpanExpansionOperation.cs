namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System.Collections.Generic;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationSpanExpansionOperation : AbstractOperation
    {
        private readonly bool _expandSpans;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;

        public TransformationSpanExpansionOperation(PublicTable targetTable, FixupCaches caches, DataStoreConfiguration configuration)
        {
            var colMappings = targetTable.GetColumnMappingsFromStage(configuration);

            _expandSpans = colMappings != null && colMappings.SpanExpansionRequired;
            _caches = caches;
            _configuration = configuration;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var r in rows)
            {
                if (!_expandSpans)
                {
                    yield return r;
                }
                else
                {
                    var wks = ((string)r["weeks"]).ToUpper();

                    var timetableId = (int)r["timetable_id"];
                    var weekDates = _caches.WeekDatesCache.Get(timetableId);

                    var weekNum = 0;
                    var occurrence = 0;

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

                                yield return vr;
                            }
                        }

                        ++weekNum;
                    }
                }
            }
        }
    }
}
