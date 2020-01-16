namespace Celcat.Verto.DataStore.Public.Schemas.TempUpsert
{
    using System;
    using System.Collections.Generic;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class TempUpsertGetRowsOperation : AbstractOperation
    {
        private readonly IEnumerable<Row> _stagingRows;
        private readonly TableColumnMappings _colMappings;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;
        private readonly bool _modifyWeekNumbers;

        public TempUpsertGetRowsOperation(
            IEnumerable<Row> stagingRows, 
            TableColumnMappings colMappings,
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            bool modifyWeekNumbers)
        {
            _stagingRows = stagingRows;
            _colMappings = colMappings;
            _caches = caches;
            _configuration = configuration;
            _modifyWeekNumbers = modifyWeekNumbers;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var r in _stagingRows)
            {
                var newRow = new Row();

                foreach (var cm in _colMappings)
                {
                    var val = cm.GetStagingValue(r, _caches, _configuration);
                    newRow[cm.PublicColumn] = val ?? DBNull.Value;
                }

                if (_modifyWeekNumbers && newRow.Contains("timetable_week"))
                {
                    newRow["timetable_week"] = (int)newRow["timetable_week"] + 1;
                }

                if (_colMappings.EventExpansionRequired)
                {
                    newRow[_colMappings.EventExpansion.PublicEventInstanceColumn] = r[_colMappings.EventExpansion.PublicEventInstanceColumn];
                    newRow[_colMappings.EventExpansion.PublicWeekColumn] = r[_colMappings.EventExpansion.PublicWeekColumn];
                    newRow[_colMappings.EventExpansion.PublicWeekOccurrenceColumn] = r[_colMappings.EventExpansion.PublicWeekOccurrenceColumn];
                }

                yield return newRow;
            }
        }
    }
}
