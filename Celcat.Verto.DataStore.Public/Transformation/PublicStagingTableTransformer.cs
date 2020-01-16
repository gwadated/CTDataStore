namespace Celcat.Verto.DataStore.Public.Transformation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.BatchDeletes;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Common.Configuration;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using Celcat.Verto.DataStore.Public.Schemas;
    using Celcat.Verto.DataStore.Public.Schemas.Event.Tables;
    using Celcat.Verto.DataStore.Public.Schemas.Misc.Tables;
    using Celcat.Verto.DataStore.Public.Staging;
    using Celcat.Verto.DataStore.Public.Transformation.Cache;
    using Celcat.Verto.DataStore.Public.Transformation.ColumnMappings;
    using Celcat.Verto.DataStore.Public.Transformation.RhinoOperations;
    using Celcat.Verto.DataStore.Public.Transformation.TableMappings;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    //// transforms rows in Public staging tables to rows in Public tables...

    internal class PublicStagingTableTransformer
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly int _timeoutSecs;
        private readonly int _maxDegreeOfParallelism;
        private readonly DataStoreConfiguration _configuration;
        private readonly int _srcTimetableId;

        private FixupCaches _caches;

        public PublicStagingTableTransformer(
            string connectionString, int timeoutSecs, DataStoreConfiguration configuration, int srcTimetableId)
        {
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _configuration = configuration;
            _srcTimetableId = srcTimetableId;
            _maxDegreeOfParallelism = _configuration.MaxDegreeOfParallelism;
        }

        public void Execute()
        {
            var tableMappings = new TableMappings.TableMappings();

            DeleteExtraneousStagingRows();
            DoParallelProcessing(tableMappings);
            DeleteOrphanedEventInstances(tableMappings);
        }

        private void DoParallelProcessing(TableMappings.TableMappings tableMappings)
        {
            _caches = new FixupCaches(_connectionString, _timeoutSecs, tableMappings);

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };
            Parallel.ForEach(tableMappings, pOptions, (tableMapping, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    TransformStagingTable(tableMapping, loopState);
                }
            });
        }

        private void TransformStagingTable(TableMapping tableMapping, ParallelLoopState loopState)
        {
            TransformInsertsAndUpdates(tableMapping, loopState);
            TransformDeletes(tableMapping, loopState);

            if (tableMapping.PublicStagingTable.Name.Equals("CT_EVENT"))
            {
                TransformEventStagingTableForInstanceData(loopState, tableMapping.PublicStagingTable);
            }

            if (tableMapping.PublicStagingTable.Name.Equals("CT_SPAN"))
            {
                TransformWeekSpanStagingTableForDates(loopState, tableMapping.PublicStagingTable);
            }

            if (_configuration.TruncatePublicStaging)
            {
                TruncateStagingTable(tableMapping, loopState);
            }
        }

        private void TransformWeekSpanStagingTableForDates(ParallelLoopState loopState, Table publicStagingTable)
        {
            var tableMapping = new TableMapping
            {
                PublicStagingTable = publicStagingTable,
                PublicTable = new WeekSpanDateTable()
            };

            TransformInsertsAndUpdates(tableMapping, loopState);
            TransformDeletes(tableMapping, loopState);
        }

        private void TransformEventStagingTableForInstanceData(ParallelLoopState loopState, Table publicStagingTable)
        {
            var tableMapping = new TableMapping
            {
                PublicStagingTable = publicStagingTable,
                PublicTable = new EventInstanceTable()
            };

            TransformInsertsAndUpdates(tableMapping, loopState);
            TransformDeletes(tableMapping, loopState);
        }

        private void DeleteOrphanedEventInstances(TableMappings.TableMappings tableMappings)
        {
            // if events have been deleted we need to remove rows from tables where those events 
            // had previously been expanded into event instances...
            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };
            Parallel.ForEach(tableMappings, pOptions, (tableMapping, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    var pt = (PublicTable)tableMapping.PublicTable;

                    var colMappings = pt.GetColumnMappingsFromStage(_configuration);
                    if (colMappings.EventExpansionRequired)
                    {
                        pt.DeleteOrphanedEventInstances(_connectionString, _timeoutSecs);
                    }
                }
            });
        }

        private void TruncateStagingTable(TableMapping tableMapping, ParallelLoopState loopState)
        {
            if (!loopState.IsExceptional)
            {
                // when we have successfully transformed the staging table we truncate it...
                var sql =
                    $"DELETE from {tableMapping.PublicStagingTable.QualifiedName} where src_timetable_id={_srcTimetableId}";

                DatabaseUtils.ExecuteSql(_connectionString, sql, _timeoutSecs);

                var sql2 = $"select count(1) from {tableMapping.PublicStagingTable.QualifiedName}";
                var count = (int)DatabaseUtils.ExecuteScalar(_connectionString, sql2, _timeoutSecs);

                if (count == 0)
                {
                    // resets the identity key values to zero...
                    DatabaseUtils.TruncateTable(
                        _connectionString, _timeoutSecs, tableMapping.PublicStagingTable.Name, PublicStagingSchema.StagingSchemaName);
                }
            }
        }

        private void TransformDeletes(TableMapping tableMapping, ParallelLoopState loopState)
        {
            if (!loopState.IsExceptional)
            {
                var etl = new TransformationEtlProcess(
                    tableMapping.PublicStagingTable,
                    (PublicTable)tableMapping.PublicTable,
                    _caches, 
                    _connectionString, 
                    _timeoutSecs, 
                    _configuration,
                    TransformationType.Delete,
                    _srcTimetableId);

                etl.Execute();
                var errors = etl.GetAllErrors().ToArray();

                if (errors.Any())
                {
                    loopState.Stop();

                    string msg =
                       $"Errors occurred during transform of deletes in public schema (table={tableMapping.PublicTable})";
                    _log.Error(msg);

                    // throw the first exception
                    throw new ApplicationException(msg, errors[0]);
                }
            }
        }

        private void TransformInsertsAndUpdates(TableMapping tableMapping, ParallelLoopState loopState)
        {
            if (!loopState.IsExceptional)
            {
                var etl = new TransformationEtlProcess(
                    tableMapping.PublicStagingTable,
                    (PublicTable)tableMapping.PublicTable,
                    _caches, 
                    _connectionString, 
                    _timeoutSecs, 
                    _configuration,
                    TransformationType.Upsert, 
                    _srcTimetableId);

                etl.Execute();
                var errors = etl.GetAllErrors().ToArray();

                if (errors.Any())
                {
                    loopState.Stop();

                    string msg =
                       $"Errors occurred during transform of inserts and updates in public schema (table={tableMapping.PublicTable})";

                    _log.Error(msg);

                    // throw the first exception
                    throw new ApplicationException(msg, errors[0]);
                }
            }
        }

        private object LookupName(ConsolidationType lookupType, string publicIdColumn, long lookupResId)
        {
            object rv = DBNull.Value;

            var rn = _caches.NameCache.Get(ConsolidationTypeUtils.ToEntity(lookupType), lookupResId, _configuration);

            if (publicIdColumn.Contains("unique"))
            {
                if (!string.IsNullOrEmpty(rn.UniqueName))
                {
                    rv = rn.UniqueName;
                }
            }
            else if (!string.IsNullOrEmpty(rn.Name))
            {
                rv = rn.Name;
            }

            return rv;
        }

        private void DeleteExtraneousStagingRows()
        {
            // delete extraneous consolidated rows from staging (in cases of consolidated items we want a max of 1 row for each)
            foreach (Entity et in Enum.GetValues(typeof(Entity)))
            {
                if (et != Entity.Unknown && EntityUtils.CanParticipateInConsolidation(et))
                {
                    string stagingTableName =
                       DatabaseUtils.GetQualifiedTableName(PublicStagingSchema.StagingSchemaName, EntityUtils.ToCtTableName(et));

                    string idFldName = EntityUtils.GetIdFldName(et);
                    string consolidatedIdFldName = ConsolidationTypeUtils.GetConsolidatedFieldName(idFldName);

                    var bd = new BatchDelete(_connectionString, _timeoutSecs, stagingTableName, PublicStagingSchema.StagingId);

                    // e.g.
                    // select s.staging_id, s.consolidated_dept_id
                    // from STAGING.CT_DEPT s
                    // inner join
                    // (
                    // select consolidated_dept_id, count(1)
                    // from STAGING.CT_DEPT 
                    // where history_status in ('I', 'U')
                    // group by consolidated_dept_id
                    // having count(1) > 1
                    // ) sc on s.consolidated_dept_id = sc.consolidated_dept_id
                    // where history_status in ('I', 'U')
                    // order by date_change desc

                    SqlBuilder sql = new SqlBuilder();
                    sql.AppendFormat("select s.{0}, s.{1}", PublicStagingSchema.StagingId, consolidatedIdFldName);
                    sql.AppendFormat("from {0} s", stagingTableName);
                    sql.Append("inner join");
                    sql.Append("(");
                    sql.AppendFormat("select {0}", consolidatedIdFldName);
                    sql.AppendFormat("from {0} sc", stagingTableName);
                    sql.AppendFormat(
                        "where {0} in ('{1}', '{2}')", 
                        HistorySchema.HistoryStatusColumnName,
                        HistorySchema.HistoryStatusInsert, 
                        HistorySchema.HistoryStatusUpdate);
                    sql.AppendFormat("group by {0}", consolidatedIdFldName);
                    sql.Append("having count(1) > 1");
                    sql.AppendFormat(") sc on s.{0} = sc.{0}", consolidatedIdFldName);
                    sql.AppendFormat(
                        "where {0} in ('{1}', '{2}')", 
                        HistorySchema.HistoryStatusColumnName,
                        HistorySchema.HistoryStatusInsert, 
                        HistorySchema.HistoryStatusUpdate);
                    sql.AppendFormat("order by date_change desc");

                    var consolidatedIds = new HashSet<long>();

                    DatabaseUtils.EnumerateResults(_connectionString, sql.ToString(), _timeoutSecs, r =>
                    {
                        long cId = (long)DatabaseUtils.SafeRead(r, consolidatedIdFldName, 0);
                        int stagingId = (int)r[PublicStagingSchema.StagingId];

                        if (cId == 0)
                        {
                            throw new ApplicationException("Unexpected consolidated Id");
                        }

                        if (consolidatedIds.Contains(cId))
                        {
                            bd.Add(stagingId);
                        }
                        else
                        {
                            consolidatedIds.Add(cId);
                        }
                    });

                    bd.Execute();
                }
            }
        }
    }
}
