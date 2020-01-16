namespace Celcat.Verto.DataStore.Admin.Admin
{
    using System;
    using System.Reflection;
    using Celcat.Verto.DataStore.Admin.Control;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using global::Common.Logging;

    internal static class AdminDatabaseValidityCheck
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks existence and validity of the admin database...
        /// </summary>
        public static void Execute(
            string connectionString, int commandTimeout, int maxDegreeOfParallelism, Pipelines pipelineOptions)
        {
            _log.Debug("Checking existence and validity of admin database");

            // Check that db is the current version...
            var ctrl = new ControlSchema(connectionString, commandTimeout, maxDegreeOfParallelism, pipelineOptions);
            int ver = ctrl.GetDatabaseVersion();
            _log.DebugFormat("Admin database version = {0}", ver);

            if (ver > 0 && ver != ControlSchema.LatestAdminDbVersion)
            {
                throw new ApplicationException($"Admin database version = {ver}, expecting {ControlSchema.LatestAdminDbVersion}");
            }

            _log.Debug("Admin database version is correct");
        }
    }
}
