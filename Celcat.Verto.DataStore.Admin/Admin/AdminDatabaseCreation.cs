namespace Celcat.Verto.DataStore.Admin.Admin
{
    using System;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Control;
    using Celcat.Verto.DataStore.Admin.Federation;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;

    internal static class AdminDatabaseCreation
    {
        private static void CreateStagingTables(string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            CreateStagingTables(StagingSchema.PrimaryStagingSchemaName, connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            CreateStagingTables(StagingSchema.SecondaryStagingSchemaName, connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
        }

        private static void CreateStagingTables(
            string schemaName, 
            string connectionString,
            int commandTimeoutSecs, 
            int maxDegreeOfParallelism, 
            Pipelines pipelineOptions)
        {
            var stage = new StagingSchema(connectionString, schemaName, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            stage.CreateStagingTables();
        }

        public static Guid Execute(string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            // admin database doesn't yet exist...
            DatabaseUtils.CreateDatabase(connectionString, commandTimeoutSecs);
            return CreateAdminDatabaseObjects(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
        }

        public static Guid CreateAdminDatabaseObjects(
            string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            // control tables...
            Guid appKey = CreateControlTables(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);

            // create empty staging tables in primary (STAGEA) and secondary (STAGEB) schemas...
            CreateStagingTables(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);

            // create the history tables (in the HISTORY schema)...
            CreateHistoryTables(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);

            // create federation tables (in the FEDERATION schema)...
            CreateFederationTables(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);

            return appKey;
        }

        private static Guid CreateControlTables(
            string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            var ctrl = new ControlSchema(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            return ctrl.CreateTables();
        }

        private static void CreateFederationTables(
            string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            var fs = new FederationSchema(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            fs.CreateTables();
        }

        private static void CreateHistoryTables(
            string connectionString, int commandTimeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            var hs = new HistorySchema(connectionString, commandTimeoutSecs, maxDegreeOfParallelism, pipelineOptions);
            hs.CreateTables();
        }
    }
}
