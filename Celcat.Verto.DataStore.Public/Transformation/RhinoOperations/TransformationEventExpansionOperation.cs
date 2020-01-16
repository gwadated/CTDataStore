namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TransformationEventExpansionOperation : AbstractOperation
    {
        private readonly TableColumnMappings _colMappings;
        private readonly bool _expandEvents;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;

        public TransformationEventExpansionOperation(PublicTable targetTable, FixupCaches caches, DataStoreConfiguration configuration)
        {
            _colMappings = targetTable.GetColumnMappingsFromStage(configuration);
            _expandEvents = _colMappings != null && _colMappings.EventExpansionRequired;
            _caches = caches;
            _configuration = configuration;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var r in rows)
            {
                if (!_expandEvents)
                {
                    yield return r;
                }
                else
                {
                    long federatedEventId = (long)r[_colMappings.EventExpansion.StagingFederatedEventIdColumn];

                    string wks = ((string)r[_colMappings.EventExpansion.StagingWeeksColumn]).ToUpper();
                    if (string.IsNullOrEmpty(wks))
                    {
                        throw new ApplicationException("weeks are null!");
                    }

                    var weekNum = 0;
                    var occurrence = 1;

                    foreach (var ch in wks)
                    {
                        if (ch == 'Y')
                        {
                            var vr = r.Clone();

                            vr[_colMappings.EventExpansion.PublicEventInstanceColumn] =
                               ColumnMappingLookup.ColumnMappingEventInstance.FabricateEventInstanceId(federatedEventId, weekNum);

                            vr[_colMappings.EventExpansion.PublicWeekColumn] = weekNum + 1;
                            vr[_colMappings.EventExpansion.PublicWeekOccurrenceColumn] = occurrence++;

                            var startEndTimes = _colMappings.Where(x => x is ColumnMappingEventStartEndTime).ToArray();
                            foreach (var s in startEndTimes)
                            {
                                vr[s.PublicColumn] = (DateTime)s.GetStagingValue(vr, _caches, _configuration);
                            }

                            yield return vr;
                        }

                        ++weekNum;
                    }
                }
            }
        }
    }
}
