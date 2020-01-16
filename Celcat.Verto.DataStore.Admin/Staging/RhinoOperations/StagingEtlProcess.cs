namespace Celcat.Verto.DataStore.Admin.Staging.RhinoOperations
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;
    using Rhino.Etl.Core.Pipelines;

    /// <summary>
    /// The Rhino ETL process that extracts timetable data as quickly 
    /// as possible from the source timetables into the staging schema
    /// of the Admin database.
    /// 
    /// A separate ETL process is run for each table of each source 
    /// timetable database.
    /// </summary>
    internal class StagingEtlProcess : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _timetableConnectionString;
        private readonly string _stageConnectionString;
        private readonly string _stageSchemaName;
        private readonly V7StagingTable _stagingTable;
        private readonly CommandTimeout _commandTimeouts;
        private readonly int _timetableId;
        private readonly RowCountAndDuration _rowCountAndDuration;

        public StagingEtlProcess(
            string timetableConnectionString, 
            string stageConnectionString, 
            V7StagingTable stagingTable,
            string stageSchemaName, 
            CommandTimeout commandTimeouts, 
            int timetableId, 
            Pipelines pipelineOptions)
        {
            if (pipelineOptions.AdminStaging.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _timetableConnectionString = timetableConnectionString;
            _stageConnectionString = stageConnectionString;
            _stagingTable = stagingTable;
            _stageSchemaName = stageSchemaName;
            _commandTimeouts = commandTimeouts;
            _timetableId = timetableId;
            _rowCountAndDuration = new RowCountAndDuration();
        }

        public RowCountAndDuration Stats => _rowCountAndDuration;

        protected override void OnFinishedProcessing(IOperation op)
        {
            base.OnFinishedProcessing(op);

            _rowCountAndDuration.RowCount += op.Statistics.OutputtedRows;
            _rowCountAndDuration.Duration += op.Statistics.Duration;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising StagingEtlProcess for table: {0}", _stagingTable.Name);

            Register(new GetTimetableDataOperation(_timetableConnectionString, _stagingTable, _timetableId));
            Register(new StagingBulkInsertOperation(_stageConnectionString, _stagingTable, _stageSchemaName, _commandTimeouts.AdminDatabase));
        }
    }
}
