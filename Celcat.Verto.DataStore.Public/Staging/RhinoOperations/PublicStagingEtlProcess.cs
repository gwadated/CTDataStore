namespace Celcat.Verto.DataStore.Public.Staging.RhinoOperations
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Common;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Operations;
    using Rhino.Etl.Core.Pipelines;

    internal class PublicStagingEtlProcess : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Table _targetTable;
        private readonly string _adminConnectionString;
        private readonly string _publicConnectionString;
        private readonly int _timeoutSecs;
        private readonly RowCountAndDuration _rowCountAndDuration;

        public PublicStagingEtlProcess(
            Table targetTable, 
            string adminConnectionString,
            string publicConnectionString, 
            int timeoutSecs, 
            Pipelines pipelineOptions)
        {
            if (pipelineOptions.PublicStaging.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _targetTable = targetTable;
            _adminConnectionString = adminConnectionString;
            _publicConnectionString = publicConnectionString;
            _timeoutSecs = timeoutSecs;
            _rowCountAndDuration = new RowCountAndDuration();
        }

        protected override void OnFinishedProcessing(IOperation op)
        {
            base.OnFinishedProcessing(op);

            _rowCountAndDuration.RowCount += op.Statistics.OutputtedRows;
            _rowCountAndDuration.Duration += op.Statistics.Duration;
        }

        public RowCountAndDuration Stats => _rowCountAndDuration;

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising PublicStagingEtlProcess for table: {0}", _targetTable.Name);

            Register(new PublicStagingGetDataOperation(_adminConnectionString, _targetTable.Name));
            Register(new PublicStagingInsertOperation(_publicConnectionString, _targetTable, _timeoutSecs));
        }
    }
}
