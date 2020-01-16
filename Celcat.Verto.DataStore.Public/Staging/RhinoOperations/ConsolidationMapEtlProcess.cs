namespace Celcat.Verto.DataStore.Public.Staging.RhinoOperations
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Pipelines;

    internal class ConsolidationMapEtlProcess : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Table _targetTable;
        private readonly string _adminConnectionString;
        private readonly string _publicConnectionString;
        private readonly int _timeoutSecs;

        public ConsolidationMapEtlProcess(
            Table targetTable, 
            string adminConnectionString,
            string publicConnectionString, 
            int timeoutSecs, 
            Pipelines pipelineOptions)
        {
            if (pipelineOptions.PublicConsolidation.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _targetTable = targetTable;
            _adminConnectionString = adminConnectionString;
            _publicConnectionString = publicConnectionString;
            _timeoutSecs = timeoutSecs;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising PublicConsolidationEtlProcess for table: {0}", _targetTable.Name);

            Register(new ConsolidationMapGetDataOperation(_adminConnectionString, _targetTable.Name));
            Register(new ConsolidationMapInsertOperation(_publicConnectionString, _targetTable));
        }
    }
}
