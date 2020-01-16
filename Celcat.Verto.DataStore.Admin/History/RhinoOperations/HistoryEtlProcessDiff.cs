namespace Celcat.Verto.DataStore.Admin.History.RhinoOperations
{
    using System.Collections.Generic;
    using System.Reflection;
    using Celcat.Verto.Common.TableDiff;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Pipelines;

    internal class HistoryEtlProcessDiff : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly Table _targetTable;
        private readonly IEnumerable<DataDiff> _differences;
        private readonly IEnumerable<string> _stagingTableColumns;
        private readonly long _logId;

        public HistoryEtlProcessDiff(
            string connectionString, 
            int timeoutSecs, 
            Table targetTable,
            IEnumerable<DataDiff> differences, 
            IEnumerable<string> stagingTableColumns,
            long logId, 
            Pipelines pipelineOptions)
        {
            if (pipelineOptions.AdminDiff.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _targetTable = targetTable;
            _differences = differences;
            _stagingTableColumns = stagingTableColumns;
            _logId = logId;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising HistoryEtlProcessDiff for table: {0}", _targetTable.Name);

            Register(new GetHistoryDiffOperation(_differences, _stagingTableColumns, _logId));
            Register(new HistoryInsertOperation(_connectionString, _targetTable, _timeoutSecs));
        }
    }
}
