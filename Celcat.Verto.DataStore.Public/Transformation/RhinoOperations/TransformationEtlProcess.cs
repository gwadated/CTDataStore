namespace Celcat.Verto.DataStore.Public.Transformation.RhinoOperations
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Pipelines;

    internal class TransformationEtlProcess : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly Table _srcTable;
        private readonly PublicTable _targetTable;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;
        private readonly TransformationType _transformationType;
        private readonly int _srcTimetableId;
        
        public TransformationEtlProcess(
            Table srcTable, 
            PublicTable targetTable, 
            FixupCaches caches,
            string connectionString, 
            int timeoutSecs, 
            DataStoreConfiguration configuration,
            TransformationType transformationType, 
            int srcTimetableId)
        {
            if (configuration.Pipelines.PublicTransformation.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _srcTable = srcTable;
            _targetTable = targetTable;
            _caches = caches;
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _configuration = configuration;
            _transformationType = transformationType;
            _srcTimetableId = srcTimetableId;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising PublicStagingEtlProcess for table: {0}", _targetTable.Name);

            if (!_configuration.DisableBulkInsertOptimisations &&
               _transformationType == TransformationType.Upsert &&
               _targetTable.IsEmpty(_connectionString, _timeoutSecs))
            {
                // optimisation here! We're inserting and the target table is empty so we can use bulk insert...

                Register(new TransformationGetDataForBulkInsertOperation(_connectionString, _srcTable, _targetTable, _caches, _configuration, _srcTimetableId));
                Register(new TransformationEventExpansionOperation(_targetTable, _caches, _configuration));
                Register(new TransformationWeekNumbersOperation(_targetTable, _configuration));
                Register(new TransformationSpanExpansionOperation(_targetTable, _caches, _configuration));
                Register(new TransformationBulkInsertTargetOperation(_connectionString, _timeoutSecs, _targetTable));
            }
            else
            {
                Register(new TransformationGetDataOperation(_connectionString, _srcTable, _transformationType, _srcTimetableId));
                Register(new TransformationUpdateTargetOperation(
                    _connectionString, _timeoutSecs, _targetTable, _caches, _configuration, _transformationType));
            }
        }
    }
}
