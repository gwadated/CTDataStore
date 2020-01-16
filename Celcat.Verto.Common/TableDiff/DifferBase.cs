namespace Celcat.Verto.Common.TableDiff
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using global::Common.Logging;

    public abstract class DifferBase
    {
        protected const string TableNumberColName = "verto_table_number";
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int _primaryKeyColCount;

        protected DifferBase(int primaryKeyColCount)
        {
            _primaryKeyColCount = primaryKeyColCount;
        }

        protected abstract SimpleTableData GetRowsWithDifferences();

        private string GeneratePrimaryKeyHash(SimpleTableRow row)
        {
            var sb = new StringBuilder();

            // skip 1st column which is table indicator...
            for (var n = 1; n < _primaryKeyColCount + 1; ++n)
            {
                if (sb.Length > 0)
                {
                    sb.Append("\t");
                }

                sb.Append(row[n]);
            }

            var result = sb.ToString();
            _log.DebugFormat("KeyHash generated = {0}", result);

            return result;
        }

        private Dictionary<string, List<SimpleTableRow>> GatherDifferences()
        {
            var results = new Dictionary<string, List<SimpleTableRow>>();

            var rows = GetRowsWithDifferences();
            foreach (SimpleTableRow r in rows)
            {
                var primaryKeyHash = GeneratePrimaryKeyHash(r);

                if (!results.TryGetValue(primaryKeyHash, out var d))
                {
                    d = new List<SimpleTableRow>();
                    results.Add(primaryKeyHash, d);
                }

                d.Add(r);
            }

            return results;
        }

        private static IEnumerable<DataDiff> CollateDifferences(Dictionary<string, List<SimpleTableRow>> diffsByPrimaryKey)
        {
            foreach (var d in diffsByPrimaryKey)
            {
                if (d.Value.Count < 1 || d.Value.Count > 2)
                {
                    _log.ErrorFormat("Expected 1 or 2 rows but found {0}", d.Value.Count);
                    throw new ApplicationException("Unexpected row count!");
                }

                var diff = new DataDiff();

                foreach (SimpleTableRow row in d.Value)
                {
                    int tableOrig = (int)row[0];
                    switch (tableOrig)
                    {
                        case 1:
                            diff.OldRow = row;
                            break;
                        case 2:
                            diff.NewRow = row;
                            break;
                        default:
                            _log.ErrorFormat("Unidentified table origin {0}", tableOrig);
                            throw new ApplicationException("Unidentified table origin");
                    }
                }

                yield return diff;
            }
        }

        public IEnumerable<DataDiff> Execute()
        {
            return CollateDifferences(GatherDifferences());
        }
    }
}
