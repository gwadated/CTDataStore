namespace Celcat.Verto.DataStore.Public.PublicDB
{
    using System;
    using System.Reflection;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Public.MetaData;
    using global::Common.Logging;

    internal static class PublicDatabaseValidityCheck
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks existence and validity of the public database...
        /// </summary>
        public static void Execute(string connectionString, int commandTimeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            _log.Debug("Checking existence and validity of public database");

            // Check that db is the current version...

            var md = new MetaDataSchema(connectionString, commandTimeout, maxDegreeOfParallelism, pipelineOptions);
            int ver = md.GetDatabaseVersion();
            _log.DebugFormat("Public database version = {0}", ver);

            if (ver > 0 && ver != MetaDataSchema.LatestPublicDbVersion)
            {
                throw new ApplicationException($"Public database version = {ver}, expecting {MetaDataSchema.LatestPublicDbVersion}");
            }

            _log.Debug("Public database version is correct");
        }
    }
}
