namespace Celcat.Verto.DataStore.Public.Schemas.TempUpsert
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;
    using Rhino.Etl.Core.Pipelines;

    internal class TempUpsertEtlProcess : EtlProcess
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly SqlConnection _sqlConnection;
        private readonly int _timeoutSecs;
        private readonly IEnumerable<Row> _stagingRows;
        private readonly TableColumnMappings _colMappings;
        private readonly FixupCaches _caches;
        private readonly DataStoreConfiguration _configuration;
        private readonly Table _tempTable;
        private readonly bool _modifyWeekNumbers;
        
        public TempUpsertEtlProcess(
            SqlConnection c, 
            int timeoutSecs, 
            IEnumerable<Row> stagingRows,
            TableColumnMappings colMappings, 
            FixupCaches caches, 
            DataStoreConfiguration configuration, 
            Table tmpTable,
            bool modifyWeekNumbers)
        {
            if (configuration.Pipelines.PublicTempUpsert.SingleThreaded)
            {
                PipelineExecuter = new SingleThreadedPipelineExecuter();
            }

            _sqlConnection = c;
            _timeoutSecs = timeoutSecs;
            _stagingRows = stagingRows;
            _colMappings = colMappings;
            _caches = caches;
            _configuration = configuration;
            _tempTable = tmpTable;
            _modifyWeekNumbers = modifyWeekNumbers;
        }

        protected override void Initialize()
        {
            _log.DebugFormat("Initialising TempUpsertEtlProcess for table: {0}", _tempTable.Name);

            Register(new TempUpsertGetRowsOperation(_stagingRows, _colMappings, _caches, _configuration, _modifyWeekNumbers));
            Register(new TempUpsertWriteOperation(_sqlConnection, _timeoutSecs, _tempTable));
        }
    }
}
