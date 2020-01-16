namespace Celcat.Verto.DataStore.Admin.History.RhinoOperations
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Pipelines;

    internal class HistoryEtlProcessBasic : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly int _timetableId;
        private readonly Table _targetHistoryTable;
        private readonly string _tableName;
        private readonly string _stagingSchemaName;
        private readonly string _historyStatus;
        private readonly long _logId;

        public HistoryEtlProcessBasic(
            string connectionString, 
            int timeoutSecs, 
            Table targetHistoryTable,
            int timetableId, 
            string stagingSchemaName, 
            string historyStatus,
            long logId, 
            Pipelines pipelineOptions)
        {
            if (pipelineOptions.AdminHistory.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _targetHistoryTable = targetHistoryTable;
            _timetableId = timetableId;
            _tableName = targetHistoryTable.Name;
            _stagingSchemaName = stagingSchemaName;
            _historyStatus = historyStatus;
            _logId = logId;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising HistoryEtlProcess for table: {0}", _targetHistoryTable.Name);

            Register(new GetHistoryOperationBasic(_connectionString, _timetableId, _tableName, _stagingSchemaName, _historyStatus, _logId));
            Register(new HistoryInsertOperation(_connectionString, _targetHistoryTable, _timeoutSecs));
        }
    }
}
