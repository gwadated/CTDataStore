namespace Celcat.Verto.DataStore.Admin.History.RhinoOperations
{
    using System;
    using System.Collections.Generic;
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.TableDiff;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;

    internal class GetHistoryDiffOperation : AbstractOperation
    {
        private readonly IEnumerable<DataDiff> _differences;
        private readonly IEnumerable<string> _stagingTableColumns;
        private readonly long _logId;

        public GetHistoryDiffOperation(IEnumerable<DataDiff> differences, IEnumerable<string> stagingTableColumns, long logId)
        {
            _differences = differences;
            _stagingTableColumns = stagingTableColumns;
            _logId = logId;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows)
        {
            foreach (var d in _differences)
            {
                var r = FromDataDiff(d);
                yield return r;
            }
        }

        private Row FromDataDiff(DataDiff d)
        {
            var r = new Row();

            SimpleTableRow sr = null;
            var statusString = string.Empty;

            switch (d.Status)
            {
                case RowStatus.Inserted:
                    statusString = HistorySchema.HistoryStatusInsert;
                    sr = d.NewRow;
                    break;

                case RowStatus.Updated:
                    statusString = HistorySchema.HistoryStatusUpdate;
                    sr = d.NewRow;
                    break;

                case RowStatus.Deleted:
                    statusString = HistorySchema.HistoryStatusDelete;
                    sr = d.OldRow;
                    break;

                case RowStatus.Unknown:
                    throw new ApplicationException("Unknown status for diff row");
            }

            if (sr != null)
            {
                int colNum = 1;   // skip the diff status flag
                foreach (var col in _stagingTableColumns)
                {
                    r[col] = sr.Get(colNum++);
                }

                r[HistorySchema.HistoryStatusColumnName] = statusString;
                r[HistorySchema.HistoryLogColumnName] = _logId;
                r[HistorySchema.HistoryFederatedColumnName] = 0;
                r[HistorySchema.HistoryAppliedColumnName] = 0;
                r[HistorySchema.HistoryStampColumnName] = DateTime.Now;
            }

            return r;
        }
    }
}
