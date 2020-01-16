namespace Celcat.Verto.DataStore.Admin.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.TableDiff;
    using Celcat.Verto.DataStore.Admin.Control;
    using Celcat.Verto.DataStore.Admin.Control.Mutex;
    using Celcat.Verto.DataStore.Admin.Federation;
    using Celcat.Verto.DataStore.Admin.Federation.FixUpHistoryIds;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Admin.Models;
    using Celcat.Verto.DataStore.Admin.SourceTimetables;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Admin.Staging.RhinoOperations;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Common.Progress;
    using global::Common.Logging;

    public sealed class AdminDatabase
    {
        private const int MutexTimerIntervalSecs = 10;

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DataStoreConfiguration _configuration;

        private bool _adminDatabaseAccessible;
        private volatile ControlMutexResult _mutex;
        private Timer _mutexTimer;
        private Guid _applicationKey;

        public AdminDatabase(DataStoreConfiguration configuration)
        {
            _configuration = configuration;
        }

        public event EventHandler<VertoProgressEventArgs> ProgressEvent;

        private CommandTimeout Timeouts => _configuration.CommandTimeouts;

        private string AdminConnectionString => _configuration.Destination.AdminDatabase.ConnectionString;

        private string PublicConnectionString => _configuration.Destination.PublicDatabase.ConnectionString;

        private IReadOnlyList<string> TimetableConnectionStrings => _configuration.TimetableConnectionStrings;

        public Guid ApplicationKey
        {
            get
            {
                if (_applicationKey == Guid.Empty)
                {
                    ControlSchema controlSchema = new ControlSchema(
                        AdminConnectionString, 
                        Timeouts.AdminDatabase,
                        _configuration.MaxDegreeOfParallelism, 
                        _configuration.Pipelines);

                    _applicationKey = controlSchema.GetApplicationKey();
                }

                return _applicationKey;
            }
        }
        
        private void EnsureAdminDatabaseExists()
        {
            var timeout = Timeouts.AdminDatabase;

            if (!DatabaseUtils.DatabaseExists(AdminConnectionString, timeout))
            {
                _applicationKey = AdminDatabaseCreation.Execute(AdminConnectionString, timeout, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);
            }
            else
            {
                var tables = DatabaseUtils.GetTablesInSchema(AdminConnectionString, timeout, false, ControlSchema.ControlSchemaName);
                if (!tables.Any())
                {
                    // database exists, but no tables in the CONTROL schema!

                    // The user may have created the database and pointed Verto at it,
                    // or we might have created the database on Azure and the create database 
                    // statement timed out (but the database was still created). 

                    // In this case we try to generate just the database objects associated with 
                    // the Admin database (we assume _all_ tables and schemas are missing)...
                    _applicationKey = AdminDatabaseCreation.CreateAdminDatabaseObjects(
                        AdminConnectionString, 
                        timeout,
                        _configuration.MaxDegreeOfParallelism, 
                        _configuration.Pipelines);
                }
            }
        }

        private IReadOnlyList<SourceTimetableData> CheckPreconditions()
        {
            _log.DebugFormat(
                "Checking preconditions in Admin database: {0}", 
                DatabaseUtils.GetConnectionDescription(AdminConnectionString));

            // check existence and validity of the source timetables...
            var srcTimetableRecords = SourceValidityCheck.Execute(
                TimetableConnectionStrings,
                Timeouts.SourceTimetables, 
                _configuration.MaxDegreeOfParallelism);

            CheckAdminAndPublicDatabasesAreDifferent();

            CheckConfigConsolidation();

            ConsolidationKeyCheck.CheckNaturalKeyColumnsInConfiguration(_configuration.Consolidation);

            // check existence of admin db...
            EnsureAdminDatabaseExists();

            _adminDatabaseAccessible = true;

            // check validity of admin db...
            AdminDatabaseValidityCheck.Execute(
                AdminConnectionString, 
                Timeouts.AdminDatabase,
                _configuration.MaxDegreeOfParallelism,
                _configuration.Pipelines);

            InitMutex();

            CheckAllChangesApplied();

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };
            Parallel.Invoke(pOptions, () =>
            {
                SourceTimetableRegistration.EnsureSourceTimetablesAreRegistered(
                    AdminConnectionString,
                    Timeouts.AdminDatabase, 
                    srcTimetableRecords, 
                    _configuration.MaxDegreeOfParallelism,
                    _configuration.Pipelines);

                StagingTablesExistence.EnsureExist(AdminConnectionString, Timeouts.AdminDatabase);
                HistoryTablesExistence.EnsureExist(AdminConnectionString, Timeouts.AdminDatabase);
                ClearTempStage();
            });

            return srcTimetableRecords;
        }

        private void CheckAdminAndPublicDatabasesAreDifferent()
        {
            _log.Debug("Checking that Admin and Public databases are different");

            var c1 = DatabaseUtils.GetConnectionDescription(AdminConnectionString);
            var c2 = DatabaseUtils.GetConnectionDescription(PublicConnectionString);

            if (c1.Equals(c2, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Admin and Public database must be different!");
            }
        }

        private void InitMutex()
        {
            _mutex = ControlMutex.Grab(AdminConnectionString, Timeouts.AdminDatabase);
            if (!_mutex.Success)
            {
                _log.ErrorFormat(
                    "Mutex failure: {0}", 
                    _mutex.Status.ToString());

                throw new ApplicationException("Could not get exclusive access to the staging database");
            }

            _mutexTimer = new Timer(
                TouchMutexOnTimerFire, 
                null,
                MutexTimerIntervalSecs * 1000, 
                MutexTimerIntervalSecs * 1000);
        }

        private void TouchMutexOnTimerFire(object state)
        {
            var mutex = _mutex;
            if (mutex != null)
            {
                _log.Debug("Touching mutex");
                ControlMutex.Touch(AdminConnectionString, Timeouts.AdminDatabase, mutex.Status.MutexValue);
            }
        }

        /// <summary>
        /// Pulls timetable data from selected tables and cols in the source timetables to the 
        /// Admin database staging area
        /// </summary>
        public void ExtractToStage()
        {
            try
            {
                var srcTimetables = CheckPreconditions();

                OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Extracting timetables to stage", Section = ProcessingSection.Staging });

                _log.DebugFormat(
                    "Extracting timetables to stage in Admin database: {0}",
                    DatabaseUtils.GetConnectionDescription(AdminConnectionString));

                // populate temp stage with timetable data...
                Log("Extracting to Stage", "Extracting timetables to temporary stage");
                RowCountAndDuration stats = ExtractTimetablesToStage(srcTimetables, StagingSchema.TemporaryStagingSchemaName);

                var msg =
                    $"Extracted {stats.RowCount} rows from {srcTimetables.Count} timetables in {stats.Duration.TotalSeconds:F2} secs";

                Log("Extracted", msg);

                OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Checking stage integrity", Section = ProcessingSection.Staging });
                CheckStageIntegrity();

                // all ok so we can safely move the existing primary staging tables to secondary staging schema...
                Log("Moving Stage", "Moving primary stage to secondary");
                DatabaseUtils.MoveTablesToNewSchema(
                    AdminConnectionString, 
                    Timeouts.AdminDatabase,
                    StagingSchema.PrimaryStagingSchemaName,
                    StagingSchema.SecondaryStagingSchemaName);

                // and move the temp schema into the primary stage, leaving temp stage empty...
                Log("Moving Stage", "Moving temporary stage to primary");
                DatabaseUtils.MoveTablesToNewSchema(
                    AdminConnectionString, 
                    Timeouts.AdminDatabase,
                    StagingSchema.TemporaryStagingSchemaName,
                    StagingSchema.PrimaryStagingSchemaName);

                // NB - we perform the above cycle so that we don't get caught out if an extract fails
                // (it's ok if it fails while extracting to the temp stage)
            }
            catch (AggregateException ex)
            {
                LogError(ex.InnerExceptions[0].Message);
                throw ex.InnerExceptions[0];
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
                throw;
            }
        }

        private void CheckStageIntegrity()
        {
            // first check that all foreign key values have an entry in the primary table...
            var sc = new StagingSchema(
                AdminConnectionString, 
                StagingSchema.TemporaryStagingSchemaName,
                Timeouts.AdminDatabase, 
                _configuration.MaxDegreeOfParallelism,
                _configuration.Pipelines);

            sc.CheckIntegrity();

            // now check that the consolidation key values are not blank...
            ConsolidationKeyCheck.Execute(
                AdminConnectionString, 
                Timeouts.AdminDatabase,
                _configuration.MaxDegreeOfParallelism, 
                _configuration.Consolidation,
                StagingSchema.TemporaryStagingSchemaName);
        }

        private long LogError(string description)
        {
            return Log("Error", description);
        }

        private long Log(string task, string description)
        {
            _log.Info(description);

            return _adminDatabaseAccessible
               ? new ControlSchema(AdminConnectionString, Timeouts.AdminDatabase, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines)
                  .Log(task, description)
               : 0;
        }

        /// <summary>
        /// Performs a 'full-data' diff between Primary and Secondary stages and updates the
        /// history tables accordingly
        /// </summary>
        public void UpdateHistory()
        {
            try
            {
                OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Updating history schema", Section = ProcessingSection.UpdatingHistory });
                long logId = Log("Updating History", "Updating history schema");

                _log.DebugFormat(
                    "Updating history tables in Admin database: {0}", DatabaseUtils.GetConnectionDescription(AdminConnectionString));

                var hs = new HistorySchema(
                    AdminConnectionString, 
                    Timeouts.AdminDatabase,
                    _configuration.MaxDegreeOfParallelism, 
                    _configuration.Pipelines);

                var pkInfo = DatabaseUtils.GetPrimaryKeyInfo(TimetableConnectionStrings.First(), Timeouts.SourceTimetables);
                AddPrimaryKeyInfoForPseudoRegisterMarkTable(pkInfo);

                var duration = hs.PerformDiff(pkInfo, logId);

                var msg = $"Updated history in {duration.TotalSeconds:F2} secs";
                Log("Updated", msg);
            }
            catch (AggregateException ex)
            {
                LogError(ex.InnerExceptions[0].Message);
                throw ex.InnerExceptions[0];
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                throw;
            }
        }

        private void AddPrimaryKeyInfoForPseudoRegisterMarkTable(Dictionary<string, PrimaryKeyInfo> pkInfo)
        {
            var p = pkInfo["CT_AT_STUDENT_MARK"];
            pkInfo.Add(StagingTablesBuilder.PseudoRegisterMarkTable, p);
        }

        private RowCountAndDuration ExtractTimetablesToStage(IReadOnlyList<SourceTimetableData> srcTimetableRecs, string stageSchemaName)
        {
            var stats = new RowCountAndDuration();

            var b = StagingTablesBuilder.Get(stageSchemaName);
            var tables = b.GetTables();

            var cs = new ControlSchema(AdminConnectionString, Timeouts.AdminDatabase, _configuration.MaxDegreeOfParallelism, _configuration.Pipelines);

            // don't use Parallel.ForEach here (no gain)
            var processedTimetables = new HashSet<int>();
            foreach (var tt in srcTimetableRecs)
            {
                _log.DebugFormat("Extracting timetable ({0}) to stage ({1})", tt.Name, stageSchemaName);

                var ttRec = cs.GetSourceTimetableRecord(tt.Identifier);
                if (ttRec == null)
                {
                    throw new ApplicationException(string.Format("Could not find source timetable registration: {0}", tt.Name));
                }

                // sanity check...
                if (processedTimetables.Contains(ttRec.Id))
                {
                    throw new ApplicationException(string.Format("Already processed a timetable with this Id: {0}", ttRec.Id));
                }

                processedTimetables.Add(ttRec.Id);

                // don't use Parallel.ForEach here (no gain)
                foreach (var t in tables)
                {
                    var stagingTable = (V7StagingTable)t;

                    using (var p = new StagingEtlProcess(
                        tt.ConnectionString, 
                        AdminConnectionString,
                        stagingTable, 
                        stageSchemaName, 
                        Timeouts, 
                        ttRec.Id, 
                        _configuration.Pipelines))
                    {
                        p.Execute();
                        stats += p.Stats;

                        var errors = p.GetAllErrors().ToArray();

                        if (errors.Any())
                        {
                            var msg = $"Errors occurred during execution of staging process: {stagingTable.Name}";
                            _log.Error(msg);

                            // throw the first exception
                            throw new ApplicationException(msg, errors[0]);
                        }
                    }
                }
            }

            return stats;
        }

        private void CheckAllChangesApplied()
        {
            _log.Debug("Checking that all changes in the History schema have been applied to PUBLIC database");

            // throw if any changes in the history tables are as yet unpublished...
            var hb = HistoryTablesBuilder.Get();

            foreach (var table in hb.GetTables())
            {
                var sql =
                    $"select count(1) cnt from {DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, table.Name)} where {HistorySchema.HistoryAppliedColumnName}=0";

                DatabaseUtils.GetSingleResult(AdminConnectionString, sql, Timeouts.AdminDatabase, r =>
                {
                    if ((int)r["cnt"] > 0)
                    {
                        throw new ApplicationException("Some entries in the History schema have not been applied to the PUBLIC database. Please rebuild the data store by deleting ADMIN and PUBLIC databases");
                    }
                });
            }
        }

        private void ClearPrimaryStage()
        {
            var stage = new StagingSchema(
                AdminConnectionString, 
                StagingSchema.PrimaryStagingSchemaName,
                Timeouts.AdminDatabase, 
                _configuration.MaxDegreeOfParallelism, 
                _configuration.Pipelines);

            if (stage.AllTablesExist())
            {
                stage.TruncateStagingTables();
            }
            else
            {
                stage.CreateStagingTables();
            }
        }

        private void ClearTempStage()
        {
            var stage = new StagingSchema(
                AdminConnectionString, 
                StagingSchema.TemporaryStagingSchemaName,
                Timeouts.AdminDatabase, 
                _configuration.MaxDegreeOfParallelism, 
                _configuration.Pipelines);

            if (stage.AllTablesExist())
            {
                stage.TruncateStagingTables();
            }
            else
            {
                stage.CreateStagingTables();
            }
        }

        private void CheckConfigConsolidation()
        {
            foreach (var entry in _configuration.Consolidation.Entries)
            {
                try
                {
                    var e = EntityUtils.FromString(entry.Entity);

                    if (!EntityUtils.CanParticipateInConsolidation(e))
                    {
                        throw new ApplicationException(string.Format("Entity in configuration, consolidation section does not participate in consolidation: {0}", entry.Entity));
                    }

                    if (!entry.None && !EntityUtils.GetValidConsolidationColumns(e).Contains(entry.Column))
                    {
                        throw new ApplicationException(string.Format("Entity in configuration, consolidation section does not have specified column: {0} - {1}", entry.Entity, entry.Column));
                    }
                }
                catch (ArgumentException)
                {
                    throw new ApplicationException(string.Format("Could not identify entity in configuration, consolidation section: {0}", entry.Entity));
                }
            }
        }

        /// <summary>
        /// Ensures that all applicable entities have master Ids assigned to them
        /// and stored in the corresponding federation tables
        /// </summary>
        public void FederateResources()
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Federating timetable resources", Section = ProcessingSection.FederatingResources });
            Log("Federating", "Federating timetable resources");

            try
            {
                DoParallelFederationProcessing();
            }
            catch (AggregateException ex)
            {
                LogError(ex.InnerExceptions[0].Message);
                throw ex.InnerExceptions[0];
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                throw;
            }
        }

        private void DoParallelFederationProcessing()
        {
            var fs = new FederationSchema(
                AdminConnectionString, 
                Timeouts.AdminDatabase,
                _configuration.MaxDegreeOfParallelism, 
                _configuration.Pipelines);

            List<Entity> entities = Enum.GetValues(typeof(Entity)).Cast<object>().Cast<Entity>().ToList();

            var b = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);
            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };

            Parallel.ForEach(entities, pOptions, (e, loopState) =>
            {
                if (!loopState.IsExceptional && EntityUtils.RequiresFederation(e))
                {
                    fs.UpdateStdFederationTable(e, b.GetTable(EntityUtils.ToCtTableName(e)));
                }
            });
        }

        public bool ConsolidationConfigChanged(ConsolidationParams c)
        {
            if (DatabaseUtils.DatabaseExists(AdminConnectionString, Timeouts.AdminDatabase))
            {
                var fs = new FederationSchema(
                    AdminConnectionString, 
                    Timeouts.AdminDatabase,
                    _configuration.MaxDegreeOfParallelism, 
                    _configuration.Pipelines);

                if (fs.ConsolidationConfigTableExists())
                {
                    return fs.ConsolidationConfigChanged(c);
                }
            }

            return false;
        }

        public void ConsolidateResources()
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = "Consolidating timetable resources", Section = ProcessingSection.ConsolidatingResources });
            Log("Consolidating", "Consolidating timetable resources");

            try
            {
                var fs = new FederationSchema(
                    AdminConnectionString, 
                    Timeouts.AdminDatabase,
                    _configuration.MaxDegreeOfParallelism, 
                    _configuration.Pipelines);

                fs.UpdateConsolidationConfigTable(_configuration.Consolidation);

                DoParallelConsolidationProcessing(fs);
            }
            catch (AggregateException ex)
            {
                LogError(ex.InnerExceptions[0].Message);
                throw ex.InnerExceptions[0];
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
                throw;
            }
        }

        private void DoParallelConsolidationProcessing(FederationSchema fs)
        {
            List<Entity> entities = Enum.GetValues(typeof(Entity)).Cast<object>().Cast<Entity>().ToList();

            var b = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism };
            Parallel.ForEach(entities, pOptions, (e, loopState) =>
            {
                if (!loopState.IsExceptional && EntityUtils.CanParticipateInConsolidation(e))
                {
                    var entry = _configuration.Consolidation.Get(e);
                    fs.UpdateConsolidationTables(e, b.GetTable(EntityUtils.ToCtTableName(e)), entry);
                }
            });
        }

        public void FinishUp()
        {
            _log.Debug("Finishing up");

            CleanupMutex();
        }

        private void CleanupMutex()
        {
            if (_mutexTimer != null)
            {
                _mutexTimer.Dispose();
                _mutexTimer = null;
            }

            if (_mutex != null)
            {
                Guid mutexValue = _mutex.Status.MutexValue;
                _mutex = null;

                if (mutexValue != Guid.Empty && _adminDatabaseAccessible)
                {
                    _log.DebugFormat("Releasing mutex: {0}", mutexValue);
                    ControlMutex.Release(AdminConnectionString, Timeouts.AdminDatabase, mutexValue);
                }
            }
        }

        /// <summary>
        /// Add federated and consolidated Ids to history tables
        /// </summary>
        public void AddHistoryFederationIds(RowStatus[] restrictToValues, int phase)
        {
            OnProgressEvent(new VertoProgressEventArgs { ProgressString = $"Adding federated IDs to history tables (phase {phase} of 2)", Section = ProcessingSection.ConsolidatingResources });
            _log.Debug("Adding federated IDs to history tables");

            var fs = new FederateHistoryTables(
                AdminConnectionString, 
                Timeouts.AdminDatabase, 
                _configuration.MaxDegreeOfParallelism, 
                restrictToValues);

            int numChanges = fs.Execute();

            _log.DebugFormat("Replaced {0} IDs in history tables with federated ID values", numChanges);
        }

        private void OnProgressEvent(VertoProgressEventArgs e)
        {
            ProgressEvent?.Invoke(this, e);
        }

        public void TruncateIfForcedRebuild()
        {
            if (_configuration.ForceRebuild)
            {
                _log.DebugFormat("'Force rebuild' option is set so dropping tables in {0}", DatabaseUtils.GetConnectionDescription(AdminConnectionString));
                DatabaseUtils.DropTablesInDatabase(AdminConnectionString, Timeouts.AdminDatabase);
            }
        }
    }
}
