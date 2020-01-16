namespace Celcat.Verto.DataStore.Admin.Federation.FixUpHistoryIds
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.TableDiff;
    using Celcat.Verto.DataStore.Admin.History;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Columns;
    using Celcat.Verto.DataStore.Common.Consolidation;
    using Celcat.Verto.DataStore.Common.Entities;
    using global::Common.Logging;

    internal class FederateHistoryTables
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _connectionString;
        private readonly int _timeoutSecs;  // command timeout secs
        private readonly RowStatus[] _restrictToValues;
        private readonly int _maxDegreeOfParallelism;

        public FederateHistoryTables(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, RowStatus[] restictToValues)
        {
            _connectionString = connectionString;
            _timeoutSecs = timeoutSecs;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _restrictToValues = restictToValues;
        }

        public int Execute()
        {
            var h = HistoryTablesBuilder.Get();
            return DoParallelProcessing(h);
        }

        private int DoParallelProcessing(HistoryTablesBuilder h)
        {
            var numChangedRows = 0;

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism };

            Parallel.ForEach(h.GetTables(), pOptions, (table, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    using (var tc = new TransactionContext(_connectionString))
                    {
                        SanityCheck(tc, (V7StagingTable)table);

                        var numChanges = AddFederatedIdValues(tc, (V7StagingTable)table);
                        Interlocked.Add(ref numChangedRows, numChanges);

                        numChanges = AddConsolidatedIdValues(tc, (V7StagingTable)table);
                        Interlocked.Add(ref numChangedRows, numChanges);

                        MarkRowsAsFixedUp(tc, table.Name);
                        tc.Commit();
                    }
                }
            });

            return numChangedRows;
        }

        private void MarkRowsAsFixedUp(TransactionContext tc, string tableName)
        {
            var sb = new SqlBuilder();

            sb.AppendFormat("update {0}", DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, tableName));
            sb.AppendFormat("set {0} = 1 where {0} = 0", HistorySchema.HistoryFederatedColumnName);
            sb.AppendFormat("and {0} in ({1})", HistorySchema.HistoryStatusColumnName, RestrictedStatusValuesAsCsv());

            DatabaseUtils.ExecuteSql(tc, sb.ToString(), _timeoutSecs);
        }

        [Conditional("DEBUG")]
        private void SanityCheck(TransactionContext tc, V7StagingTable historyTable)
        {
            if (!_restrictToValues.Contains(RowStatus.Deleted))
            {
                _log.DebugFormat("Checking Id values exist before fixup in history table: {0}", historyTable.Name);

                foreach (var c in historyTable.FederatedIdCols)
                {
                    if (!OmitFromSanityCheck(historyTable, c))
                    {
                        Entity entity = c.Entity;

                        if (entity != Entity.Unknown)
                        {
                            string columnName = c.OriginalColName;

                            // e.g.
                            // select count(1) from HISTORY.CT_ROOM h
                            // where h.history_status <> 'D' and h.history_federated = 0 and h.dept_id is not null and 
                            // not exists (select * from FEDERATION.DEPT f
                            // where item_id = h.dept_id and src_timetable_id = h.src_timetable_id)
                            var sb = new SqlBuilder();
                            sb.AppendFormat(
                                "select count(1) from {0} h",
                                DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, historyTable.Name));

                            sb.AppendFormat(
                                "where h.{0} <> '{1}' and h.{2} = 0 and h.{3} is not null and not exists",
                                HistorySchema.HistoryStatusColumnName, 
                                HistorySchema.HistoryStatusDelete,
                                HistorySchema.HistoryFederatedColumnName, 
                                columnName);

                            sb.Append("(select * from");

                            string qualifiedFedTable = DatabaseUtils.GetQualifiedTableName(
                                FederationSchema.FederationSchemaName, EntityUtils.ToFederationTableName(entity));

                            sb.AppendFormat("{0} f", qualifiedFedTable);

                            sb.AppendFormat(
                                "where {0} = h.{1} and {2} = h.{2})",
                                FederationSchema.ItemIdCol, 
                                columnName, 
                                ColumnConstants.SrcTimetableIdColumnName);

                            int count = Convert.ToInt32(DatabaseUtils.ExecuteScalar(tc, sb.ToString(), _timeoutSecs));
                            if (count > 0)
                            {
                                throw new ApplicationException(
                                    $"{count} incompatible rows found in {historyTable.Name}, column {columnName}");
                            }
                        }
                    }
                }
            }
        }

        private bool OmitFromSanityCheck(V7StagingTable historyTable, FederationDefinition federationDefinition)
        {
            return false;
        }

        private int AddFederatedIdValues(TransactionContext tc, V7StagingTable table)
        {
            _log.DebugFormat("Adding federated Id values to history table: {0}", table.Name);

            var numChanges = 0;

            foreach (var c in table.FederatedIdCols)
            {
                Entity entity = c.Entity;
                var origColumnName = c.OriginalColName;
                var fedColumnName = c.FederationIdColName;

                if (entity == Entity.Unknown)
                {
                    // this is a generic column such as "resource_id" where we don't know
                    // the entity type without reference to the value stored in its corresponding
                    // 'type' column (e.g. "resource_type")...
                    var entityDefCol = c.EntityDefinitionColName;
                    var entityTypesUsed = GetEntityTypedUsed(tc, table.Name, entityDefCol);

                    foreach (var ct7EType in entityTypesUsed)
                    {
                        // e.g.
                        // update h
                        // set h.federated_resource_id = f.ctds_id
                        // from HISTORY.CT_AT_AUX_MARK h
                        // inner join 
                        // FEDERATION.ROOM f 
                        // on h.resource_id = f.item_id and h.src_timetable_id = f.src_timetable_id
                        // and h.resource_type = 604
                        // and h.history_federated = 0
                        // and h.history_status in ('I', 'U')
                        var et = EntityUtils.FromCt7Entity(ct7EType);

                        var sb = new SqlBuilder();
                        sb.Append("update h");
                        sb.AppendFormat("set h.{0} = f.{1}", fedColumnName, FederationSchema.MasterIdColName);
                        sb.AppendFormat(
                            "from {0} h", DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, table.Name));
                        sb.Append("inner join");

                        string qualifiedFedTable = DatabaseUtils.GetQualifiedTableName(
                            FederationSchema.FederationSchemaName, EntityUtils.ToFederationTableName(et));

                        sb.AppendFormat("{0} f", qualifiedFedTable);

                        sb.AppendFormat(
                            "on h.{0} = f.{1} and h.{2} = f.{2}",
                            origColumnName, 
                            FederationSchema.ItemIdCol, 
                            ColumnConstants.SrcTimetableIdColumnName);

                        sb.AppendFormat("and h.{0} = {1}", entityDefCol, (int)ct7EType);
                        sb.AppendFormat("and h.{0} = 0", HistorySchema.HistoryFederatedColumnName);
                        sb.AppendFormat("and h.{0} in ({1})", HistorySchema.HistoryStatusColumnName, RestrictedStatusValuesAsCsv());

                        numChanges += DatabaseUtils.ExecuteSql(tc, sb.ToString(), _timeoutSecs);
                    }
                }
                else
                {
                    // e.g.
                    // update h
                    // set h.federated_dept_id = f.ctds_id
                    // from HISTORY.CT_ROOM h
                    // inner join 
                    // FEDERATION.DEPT f 
                    // on h.dept_id = f.item_id and h.src_timetable_id = f.src_timetable_id
                    // and h.history_federated = 0
                    // and h.history_status in ('I', 'U')
                    var sb = new SqlBuilder();
                    sb.Append("update h");
                    sb.AppendFormat("set h.{0} = f.{1}", fedColumnName, FederationSchema.MasterIdColName);
                    sb.AppendFormat(
                        "from {0} h", DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, table.Name));
                    sb.Append("inner join");

                    string qualifiedFedTable = DatabaseUtils.GetQualifiedTableName(
                        FederationSchema.FederationSchemaName, EntityUtils.ToFederationTableName(entity));

                    sb.AppendFormat("{0} f", qualifiedFedTable);

                    sb.AppendFormat(
                        "on h.{0} = f.{1} and h.{2} = f.{2}",
                        origColumnName, 
                        FederationSchema.ItemIdCol, 
                        ColumnConstants.SrcTimetableIdColumnName);

                    sb.AppendFormat("and h.{0} = 0", HistorySchema.HistoryFederatedColumnName);
                    sb.AppendFormat("and h.{0} in ({1})", HistorySchema.HistoryStatusColumnName, RestrictedStatusValuesAsCsv());

                    numChanges += DatabaseUtils.ExecuteSql(tc, sb.ToString(), _timeoutSecs);
                }
            }

            return numChanges;
        }

        private string RestrictedStatusValuesAsCsv()
        {
            var sb = new StringBuilder();

            foreach (var v in _restrictToValues)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.AppendFormat("'{0}'", HistorySchema.HistoryStatusToString(v));
            }

            return sb.ToString();
        }

        private IEnumerable<Ct7Entity> GetEntityTypedUsed(TransactionContext tc, string tableName, string entityDefColName)
        {
            var result = new List<Ct7Entity>();

            var sql =
                $"select distinct {entityDefColName} from {DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, tableName)}";

            DatabaseUtils.EnumerateResults(tc, sql, _timeoutSecs, rec =>
            {
                var val = (Ct7Entity)rec[entityDefColName];
                if (EntityUtils.FromCt7Entity(val) != Entity.Unknown)
                {
                    result.Add(val);
                }
            });

            return result;
        }

        private int AddConsolidatedIdValues(TransactionContext tc, V7StagingTable table)
        {
            _log.DebugFormat("Adding consolidated Id values to history table: {0}", table.Name);

            var numChanges = 0;

            foreach (var c in table.ConsolidatedIdCols)
            {
                var ctype = c.ConsolidationType;
                var consolidatedColumnName = c.ConsolidationIdColName;
                var federatedColumnName = FindFederatedColumnName(c, table);

                if (ctype == ConsolidationType.None)
                {
                    // this is a generic column such as "resource_id" where we don't know
                    // the entity type without reference to the value stored in its corresponding
                    // 'type' column (e.g. "resource_type")...
                    var entityDefCol = c.EntityDefinitionColName;
                    var entityTypesUsed = GetEntityTypedUsed(tc, table.Name, entityDefCol);

                    foreach (var ct7EType in entityTypesUsed)
                    {
                        // e.g.
                        // update h
                        // set h.consolidated_resource_id = cd.consolidation_id
                        // from HISTORY.CT_AT_AUX_MARK h
                        // inner join 
                        // FEDERATION.CONSOLIDATION_DETAIL_ROOM cd 
                        // on h.federated_resource_id = cd.ctds_id 
                        // and h.history_federated = 0
                        // and h.resource_type = 604
                        // and h.history_status in ('I', 'U')
                        var sb = new SqlBuilder();
                        sb.Append("update h");
                        sb.AppendFormat("set h.{0} = cd.{1}", consolidatedColumnName, FederationSchema.ConsolidationIdColName);
                        sb.AppendFormat(
                            "from {0} h", DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, table.Name));
                        sb.Append("inner join");

                        string qualifiedConsolidationDetailTable =
                           DatabaseUtils.GetQualifiedTableName(
                               FederationSchema.FederationSchemaName, EntityUtils.ToConsolidationDetailTableName(EntityUtils.FromCt7Entity(ct7EType)));

                        sb.AppendFormat("{0} cd", qualifiedConsolidationDetailTable);
                        sb.AppendFormat("on h.{0} = cd.{1}", federatedColumnName, FederationSchema.MasterIdColName);
                        sb.AppendFormat("and h.{0} = 0", HistorySchema.HistoryFederatedColumnName);
                        sb.AppendFormat("and h.{0} = {1}", entityDefCol, (int)ct7EType);
                        sb.AppendFormat("and h.{0} in ({1})", HistorySchema.HistoryStatusColumnName, RestrictedStatusValuesAsCsv());

                        numChanges += DatabaseUtils.ExecuteSql(tc, sb.ToString(), _timeoutSecs);
                    }
                }
                else
                {
                    // e.g.
                    // update h
                    // set h.consolidated_dept_id = cd.consolidation_id
                    // from HISTORY.CT_ROOM h
                    // inner join 
                    // FEDERATION.CONSOLIDATION_DETAIL_DEPT cd 
                    // on h.federated_dept_id = cd.ctds_id 
                    // and h.history_federated = 0
                    // and h.history_status in ('I', 'U')
                    var sb = new SqlBuilder();
                    sb.Append("update h");
                    sb.AppendFormat("set h.{0} = cd.{1}", consolidatedColumnName, FederationSchema.ConsolidationIdColName);
                    sb.AppendFormat(
                        "from {0} h", DatabaseUtils.GetQualifiedTableName(HistorySchema.HistorySchemaName, table.Name));
                    sb.Append("inner join");

                    var qualifiedConsolidationDetailTable =
                       DatabaseUtils.GetQualifiedTableName(
                           FederationSchema.FederationSchemaName,
                           EntityUtils.ToConsolidationDetailTableName(ConsolidationTypeUtils.ToEntity(ctype)));

                    sb.AppendFormat("{0} cd", qualifiedConsolidationDetailTable);
                    sb.AppendFormat("on h.{0} = cd.{1}", federatedColumnName, FederationSchema.MasterIdColName);
                    sb.AppendFormat("and h.{0} = 0", HistorySchema.HistoryFederatedColumnName);
                    sb.AppendFormat("and h.{0} in ({1})", HistorySchema.HistoryStatusColumnName, RestrictedStatusValuesAsCsv());

                    numChanges += DatabaseUtils.ExecuteSql(tc, sb.ToString(), _timeoutSecs);
                }
            }

            return numChanges;
        }

        private string FindFederatedColumnName(ConsolidationDefinition consolidationDefinition, V7StagingTable table)
        {
            var fd = table.FederatedIdCols.FirstOrDefault(
                x => x.OriginalColName.Equals(consolidationDefinition.OriginalColName));

            if (fd == null)
            {
                throw new ApplicationException(
                    $"Unable to find federation column corresponding to consolidation column: {table.Name}.{consolidationDefinition.ConsolidationIdColName}");
            }

            return fd.FederationIdColName;
        }
    }
}
