namespace Celcat.Verto.DataStore.Admin.Staging
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging.CheckingIntegrity;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Schemas;
    using global::Common.Logging;
    
    internal class StagingSchema : SchemaBase
    {
        public const string PrimaryStagingSchemaName = "STAGEA";
        public const string SecondaryStagingSchemaName = "STAGEB";
        public const string TemporaryStagingSchemaName = "STAGETMP";
        public const int MaxWeeksInTimetable = 56;
        public const int MaxPeriodsPerDayInTimetable = 36;

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _schemaName;

        internal StagingSchema(
            string connectionString, 
            string schemaName,
            int timeoutSecs, 
            int maxDegreeOfParallelism, 
            Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
            _schemaName = schemaName;
        }

        /// <summary>
        /// Creates empty staging tables (or truncates existing ones). Assumes the database exists
        /// </summary>
        public void CreateStagingTables()
        {
            _log.DebugFormat("Creating staging tables in schema: {0}", _schemaName);

            DropTablesInSchema(); // just in case
            InternalCreateEmptyStagingTables();
        }

        /// <summary>
        /// Truncates staging tables. Assumes the database (and staging tables) exist
        /// </summary>
        public void TruncateStagingTables()
        {
            _log.DebugFormat("Truncating staging tables in schema: {0}", _schemaName);

            DatabaseUtils.TruncateTablesInSchema(ConnectionString, TimeoutSecs, _schemaName);
        }

        private void InternalCreateEmptyStagingTables()
        {
            EnsureSchemaCreated();

            var builder = StagingTablesBuilder.Get(_schemaName);
            builder.Execute(ConnectionString, TimeoutSecs);
        }

        /// <summary>
        /// Determines if staging tables exist in this schema
        /// </summary>
        public bool AllTablesExist()
        {
            return AllStagingTablesExist();
        }

        private bool AllStagingTablesExist()
        {
            var b = StagingTablesBuilder.Get(_schemaName);
            return DoParallelProcessing(b);
        }

        private bool DoParallelProcessing(StagingTablesBuilder b)
        {
            var rv = true;

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
            Parallel.ForEach(b.GetTableNames(), pOptions, (t, loopState) =>
            {
                if (!loopState.IsExceptional && !DatabaseUtils.TableExists(ConnectionString, TimeoutSecs, t, _schemaName))
                {
                    rv = false;
                    loopState.Stop();
                }
            });

            return rv;
        }

        public void CheckIntegrity()
        {
            _log.DebugFormat("Checking stage integrity: {0}", _schemaName);
            var checker = new StageIntegrityCheck(ConnectionString, TimeoutSecs, MaxDegreeOfParallelism, _schemaName);
            checker.Execute();
        }

        protected override string SchemaName => _schemaName;
    }
}
