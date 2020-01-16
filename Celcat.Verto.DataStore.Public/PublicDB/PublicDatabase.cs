namespace Celcat.Verto.DataStore.Public.PublicDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Common;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Common.Progress;
    using Celcat.Verto.DataStore.Public.MetaData;
    using Celcat.Verto.DataStore.Public.Schemas.Event;
    using Celcat.Verto.DataStore.Public.Schemas.Misc;
    using Celcat.Verto.DataStore.Public.Staging;
    using Celcat.Verto.DataStore.Public.Staging.RhinoOperations;
    using Celcat.Verto.DataStore.Public.Transformation;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    public sealed class PublicDatabase
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DataStoreConfiguration _configuration;

        public event EventHandler<VertoProgressEventArgs> ProgressEvent;

        public PublicDatabase(DataStoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        private CommandTimeout Timeouts => _configuration.CommandTimeouts;

        private string AdminConnectionString => _configuration.Destination.AdminDatabase.ConnectionString;

        private string PublicConnectionString => _configuration.Destination.PublicDatabase.ConnectionString;

        private void EnsureAdminDatabaseExists(Guid adminAppKey)
        {
            int timeout = Timeouts.PublicDatabase;
            string connectionString = PublicConnectionString;

            if (!DatabaseUtils.DatabaseExists(connectionString, timeout))
            {
                PublicDatabaseCreation.Execute(
                    adminAppKey, connectionString, timeout, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);
            }
            else
            {
                var tables = DatabaseUtils.GetTablesInSchema(
                    connectionString, timeout, false, MetaDataSchema.MetadataSchemaName);

                if (!tables.Any())
                {
                    // database exists, but no tables in the METADATA schema!

                    // In this case we try to generate just the database objects associated with 
                    // the Public database (we assume _all_ tables and schemas are missing)...
                    PublicDatabaseCreation.CreatePublicDatabaseObjects(
                        adminAppKey, connectionString, timeout, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);
                }
            }
        }

        public void CheckIdenticalAppKey(Guid adminAppKey)
        {
            if (DatabaseUtils.DatabaseExists(PublicConnectionString, Timeouts.PublicDatabase))
            {
                var tables = DatabaseUtils.GetTablesInSchema(
                    PublicConnectionString, Timeouts.PublicDatabase, false, MetaDataSchema.MetadataSchemaName);

                if (tables.Any())
                {
                    MetaDataSchema mdSchema = new MetaDataSchema(
                        PublicConnectionString, Timeouts.PublicDatabase, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);

                    Guid existingAppKey = mdSchema.GetApplicationKey();
                    if (existingAppKey != adminAppKey)
                    {
                        throw new ApplicationException(
                            $"Application key in Public database ({DatabaseUtils.GetConnectionDescription(PublicConnectionString)}) is not the same as that in the Admin database ({DatabaseUtils.GetConnectionDescription(AdminConnectionString)})");
                    }
                }
            }
        }

        private void CheckPreconditions(Guid adminAppKey)
        {
            _log.DebugFormat("Checking preconditions in Public database: {0}", DatabaseUtils.GetConnectionDescription(PublicConnectionString));

            EnsureAdminDatabaseExists(adminAppKey);

            // check validity of public db...
            PublicDatabaseValidityCheck.Execute(
                PublicConnectionString, Timeouts.PublicDatabase, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);

            CheckAllChangesApplied();
        }

        private void CheckAllChangesApplied()
        {
            // throw if any rows in the staging tables are as yet untransformed (i.e. they're still there)...

            var b = PublicStagingTablesBuilder.Get();
            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };
            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                var sql = $"select count(1) from {table.QualifiedName}";
                DatabaseUtils.GetSingleResult(PublicConnectionString, sql, Timeouts.PublicDatabase, r =>
                {
                   var recCount = (int)r[0];
                   if (recCount > 0)
                   {
                       throw new ApplicationException($"{recCount} staging rows not yet applied: {table.Name}");
                   }
                });
            });
        }

        public void PopulateStage(Guid adminAppKey)
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Populating public stage", Section = ProcessingSection.StagingPublic });
            _log.Debug("Populating public stage");

            CheckPreconditions(adminAppKey);

            var b = PublicStagingTablesBuilder.Get();

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };
            var stats = DoParallelProcessingCreateStage(b, pOptions);
            DoParallelProcessingFillWeeks(b, pOptions);

            if (stats.RowCount > 0)
            {
                // only mark as applied _after_ the PUBLIC staging schema has been successfully populated...
                DoParallelMarkHistoryApplied(b, pOptions);
            }
        }

        private void DoParallelProcessingFillWeeks(PublicStagingTablesBuilder b, ParallelOptions pOptions)
        {
            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    var cols = b.GetColumnNames(table.Name);
                    if (cols.Contains("weeks") && cols.Contains("federated_event_id") && !table.Name.Equals("CT_EVENT"))
                    {
                        FillWeeksCol(table);
                    }
                }
            });
        }

        private void FillWeeksCol(Table table)
        {
            SqlBuilder sb = new SqlBuilder();

            sb.AppendFormat(
                "update er set weeks=e.weeks from {0} er inner join {1} e on e.federated_event_id = er.federated_event_id",
                table.QualifiedName, 
                DatabaseUtils.GetQualifiedTableName(PublicStagingSchema.StagingSchemaName, EntityUtils.ToCtTableName(Entity.Event)));

            sb.Append("where er.weeks is null;");

            sb.AppendFormat(
                "update er set weeks=e.weeks from {0} er inner join {1} e on e.event_id = er.federated_event_id",
                table.QualifiedName, 
                DatabaseUtils.GetQualifiedTableName(EventSchema.EventSchemaName, EntityUtils.ToFederationTableName(Entity.Event)));

            sb.Append("where er.weeks is null");
            sb.AppendFormat(
                "and {0} in ('{1}', '{2}')", 
                HistorySchema.HistoryStatusColumnName,
                HistorySchema.HistoryStatusInsert, 
                HistorySchema.HistoryStatusUpdate);

            DatabaseUtils.ExecuteSql(PublicConnectionString, sb.ToString(), Timeouts.PublicDatabase);
        }

        private void DoParallelMarkHistoryApplied(PublicStagingTablesBuilder b, ParallelOptions pOptions)
        {
            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    MarkHistoryRowsAsApplied(table.Name);
                }
            });
        }

        private RowCountAndDuration DoParallelProcessingCreateStage(PublicStagingTablesBuilder b, ParallelOptions pOptions)
        {
            RowCountAndDuration result = new RowCountAndDuration();

            object locker = new object();

            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    var p = new PublicStagingEtlProcess(
                        table, AdminConnectionString, PublicConnectionString, Timeouts.PublicDatabase, _configuration.Pipelines);

                    p.Execute();

                    var errors = p.GetAllErrors().ToArray();

                    if (errors.Any())
                    {
                        loopState.Stop();

                        string msg = $"Errors occurred during execution of public staging process: {table.Name}";
                        _log.Error(msg);

                        // throw the first exception
                        throw new ApplicationException(msg, errors[0]);
                    }

                    lock (locker)
                    {
                        result += p.Stats;
                    }
                }
            });

            return result;
        }

        private void CopyConsolidationMap()
        {
            _log.Debug("Copying consolidation map into public stage");

            // copies consolidation tables into the staging schema...
            // (the consolidation tables are already empty)

            var b = new ConsolidationTablesBuilder();
            DoParallelCopyConsolidationMap(b);
        }

        private void DoParallelCopyConsolidationMap(ConsolidationTablesBuilder b)
        {
            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };
            Parallel.ForEach(b.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    var p = new ConsolidationMapEtlProcess(
                        table, AdminConnectionString, PublicConnectionString, Timeouts.PublicDatabase, _configuration.Pipelines);

                    p.Execute();

                    var errors = p.GetAllErrors().ToArray();
                    if (errors.Any())
                    {
                        loopState.Stop();

                        string msg = $"Errors occurred during execution of consolidation map staging process: {table.Name}";
                        _log.Error(msg);

                        // throw the first exception
                        throw new ApplicationException(msg, errors[0]);
                    }
                }
            });
        }

        private void MarkHistoryRowsAsApplied(string tableName)
        {
            string sql =
                $"update {DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, tableName)} set {HistorySchema.HistoryAppliedColumnName}=1 where {HistorySchema.HistoryAppliedColumnName}=0";

            DatabaseUtils.ExecuteSql(AdminConnectionString, sql, Timeouts.AdminDatabase);
        }

        public void TransformStage()
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Transforming public stage", Section = ProcessingSection.TransformingPublic });

            // move all of the data from the Public STAGING schema to the RESOURCE, ATTENDANCE schemas etc
            var srcTimetableIds = GetSourceTimetableIds();

            // don't do parallel here - it can prevent proper updating of target tables
            foreach (var ttId in srcTimetableIds)
            {
                var t = new PublicStagingTableTransformer(PublicConnectionString, Timeouts.PublicDatabase, _configuration, (int)ttId);
                t.Execute();
            }
        }

        private IEnumerable<long> GetSourceTimetableIds()
        {
            var results = new List<long>();

            var sql =
                $"select distinct src_timetable_id from {DatabaseUtils.GetQualifiedTableName(PublicStagingSchema.StagingSchemaName, "CT_CONFIG")}";

            DatabaseUtils.EnumerateResults(PublicConnectionString, sql, Timeouts.PublicDatabase, r =>
            {
                int id = (int)r["src_timetable_id"];
                if (!results.Contains(id))
                {
                    results.Add(id);
                }
            });

            sql =
                $"select distinct timetable_id from {DatabaseUtils.GetQualifiedTableName(MiscSchema.MiscSchemaName, "TIMETABLE_CONFIG")}";

            DatabaseUtils.EnumerateResults(PublicConnectionString, sql, Timeouts.PublicDatabase, r =>
            {
                var id = (long)r["timetable_id"];

                if (!results.Contains(id))
                {
                    results.Add(id);
                }
            });

            return results;
        }

        private void OnProgressEvent(VertoProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        public void TruncateIfForcedRebuild()
        {
            if (_configuration.ForceRebuild)
            {
                _log.DebugFormat("'Force rebuild' option is set so dropping tables in {0}", DatabaseUtils.GetConnectionDescription(PublicConnectionString));
                DatabaseUtils.DropTablesInDatabase(PublicConnectionString, Timeouts.PublicDatabase);
            }
        }

        public bool TablesExist()
        {
            return
               DatabaseUtils.DatabaseExists(PublicConnectionString, Timeouts.PublicDatabase) &&
               DatabaseUtils.TableExists(PublicConnectionString, Timeouts.PublicDatabase, "TIMETABLE_CONFIG", "MISC");
        }
    }
}
