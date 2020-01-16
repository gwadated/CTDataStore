namespace Celcat.Verto.DataStore.Admin.Staging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using global::Common.Logging;
    
    internal static class ConsolidationKeyCheck
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Checks that the column names specified in the configuration, consolidation
        /// section are valid for the given entity
        /// </summary>
        public static void CheckNaturalKeyColumnsInConfiguration(ConsolidationParams consolidationParams)
        {
            _log.Debug("Checking that consolidation key columns are valid in configuration");

            StagingTablesBuilder b = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);

            if (consolidationParams.Enabled)
            {
                var entitiesUsed = new List<Entity>();

                foreach (Entity entity in Enum.GetValues(typeof(Entity)))
                {
                    if (EntityUtils.CanParticipateInConsolidation(entity))
                    {
                        if (entitiesUsed.Contains(entity))
                        {
                            throw new ApplicationException(string.Format("Entity declared more than once in consolidation configuration: {0}", entity));
                        }

                        entitiesUsed.Add(entity);

                        var entry = consolidationParams.Get(entity);
                        if (entry != null && !entry.None)
                        {
                            string stagingTableName = EntityUtils.ToCtTableName(entity);

                            var table = b.GetTable(stagingTableName);

                            if (!table.ColumnExists(entry.Column))
                            {
                                throw new ApplicationException(string.Format("The specified consolidation column ({0}) does not exist in the entity: {1}", entry.Column, entity));
                            }
                        }
                    }
                }
            }
        }

        public static void Execute(
            string connectionString, 
            int timeoutSecs,
            int maxDegreeOfParallelism, 
            ConsolidationParams consolidationParams,
            string stagingSchemaName)
        {
            if (consolidationParams.Enabled)
            {
                _log.Debug("Checking that consolidation keys aren't blank");
                DoParallelProcessing(connectionString, timeoutSecs, maxDegreeOfParallelism, consolidationParams, stagingSchemaName);
            }
        }

        private static void DoParallelProcessing(
            string connectionString, 
            int timeoutSecs,
            int maxDegreeOfParallelism, 
            ConsolidationParams consolidationParams,
            string stagingSchemaName)
        {
            List<Entity> entities = Enum.GetValues(typeof(Entity)).Cast<object>().Cast<Entity>().ToList();

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism };
            Parallel.ForEach(entities, pOptions, (e, loopState) =>
            {
                if (!loopState.IsExceptional && EntityUtils.CanParticipateInConsolidation(e))
                {
                    var entry = consolidationParams.Get(e);

                    if (!entry.None)
                    {
                        var qualifiedStagingTableName =
                            DatabaseUtils.GetQualifiedTableName(stagingSchemaName, EntityUtils.ToCtTableName(e));

                        var naturalKey = entry.Column;

                        var sql =
                            $"select count(1) from {qualifiedStagingTableName} where {naturalKey} is null or RTRIM(LTRIM({naturalKey})) = ''";

                        var count = Convert.ToInt32(DatabaseUtils.ExecuteScalar(connectionString, sql, timeoutSecs));
                        if (count > 0)
                        {
                            loopState.Stop();

                            var msg =
                                $"There are {count} rows in {qualifiedStagingTableName} with empty consolidation values (column='{naturalKey}')";

                            _log.Error(msg);

                            throw new ApplicationException(msg);
                        }
                    }
                }
            });
        }
    }
}
