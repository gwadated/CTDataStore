namespace Celcat.Verto.DataStore.Public.Staging
{
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Schemas;
    using global::Common.Logging;

    internal class PublicStagingSchema : SchemaBase
    {
        public const string StagingSchemaName = "STAGING";
        public const string HistoryStampPublicColumnName = "history_stamp_public";
        public const string StagingId = "staging_id";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PublicStagingSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }

        public void CreateTables()
        {
            _log.DebugFormat("Creating staging tables in schema: {0}", StagingSchemaName);

            DropTablesInSchema();
            InternalCreateEmptyTables();
        }

        private void InternalCreateEmptyTables()
        {
            EnsureSchemaCreated();

            PublicStagingTablesBuilder.Get().Execute(ConnectionString, TimeoutSecs);

            // add the consolidation tables too...
            new ConsolidationTablesBuilder().Execute(ConnectionString, TimeoutSecs);
        }

        protected override string SchemaName => StagingSchemaName;
    }
}
